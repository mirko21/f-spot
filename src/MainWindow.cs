using Gdk;
using Gtk;
using GtkSharp;
using Glade;
using Gnome;
using System;
using System.Text;

using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using FSpot;

using LibGPhoto2;

public class MainWindow {

        public static MainWindow Toplevel;

	Db db;

	TagSelectionWidget tag_selection_widget;
	[Glade.Widget] Gtk.Window main_window;

	[Glade.Widget] Gtk.HPaned main_hpaned;
	[Glade.Widget] Gtk.VBox left_vbox;
	[Glade.Widget] Gtk.VBox group_vbox;
	[Glade.Widget] Gtk.VBox view_vbox;

	[Glade.Widget] Gtk.VBox toolbar_vbox;

	[Glade.Widget] ScrolledWindow icon_view_scrolled;
	[Glade.Widget] Box photo_box;
	[Glade.Widget] Notebook view_notebook;
	[Glade.Widget] ScrolledWindow tag_selection_scrolled;

	//
	// Menu items
	//
	[Glade.Widget] MenuItem version_menu_item;
	[Glade.Widget] MenuItem create_version_menu_item;
	[Glade.Widget] MenuItem delete_version_menu_item;
	[Glade.Widget] MenuItem rename_version_menu_item;

	[Glade.Widget] MenuItem delete_selected_tag;
	[Glade.Widget] MenuItem edit_selected_tag;

	[Glade.Widget] MenuItem attach_tag_to_selection;
	[Glade.Widget] MenuItem remove_tag_from_selection;

	[Glade.Widget] MenuItem copy;
	[Glade.Widget] MenuItem rotate_left;
	[Glade.Widget] MenuItem rotate_right;
	[Glade.Widget] MenuItem update_thumbnail;
	[Glade.Widget] MenuItem delete_from_drive;

	[Glade.Widget] MenuItem send_mail;
	[Glade.Widget] MenuItem export;
	[Glade.Widget] MenuItem print;
	[Glade.Widget] MenuItem select_none;
	[Glade.Widget] MenuItem copy_location;
	[Glade.Widget] MenuItem adjust_color;
	[Glade.Widget] MenuItem exif_data;
	[Glade.Widget] MenuItem sharpen;
	[Glade.Widget] MenuItem remove_from_catalog;

	[Glade.Widget] CheckMenuItem display_toolbar;
	[Glade.Widget] CheckMenuItem display_sidebar;
	[Glade.Widget] CheckMenuItem display_timeline;
	[Glade.Widget] CheckMenuItem display_dates_menu_item;
	[Glade.Widget] CheckMenuItem display_tags_menu_item;

	[Glade.Widget] MenuItem set_as_background;

	[Glade.Widget] MenuItem attach_tag;
	[Glade.Widget] MenuItem remove_tag;
	[Glade.Widget] MenuItem find_tag;
	
	[Glade.Widget] Scale zoom_scale;

	[Glade.Widget] VPaned info_vpaned;

	[Glade.Widget] Gtk.Image near_image;
	[Glade.Widget] Gtk.Image far_image;

	[Glade.Widget] Gtk.HBox tagbar;
	[Glade.Widget] Gtk.Entry tag_entry;

	Gtk.Toolbar toolbar;

	PhotoVersionMenu versions_submenu;

	Gtk.ToggleButton browse_button;
	Gtk.ToggleButton view_button;

	InfoBox info_box;
	FSpot.InfoDisplay info_display;
	QueryView icon_view;
	PhotoView photo_view;
	FSpot.FullScreenView fsview;
	FSpot.PhotoQuery query;
	FSpot.GroupSelector group_selector;
	
	FSpot.Delay slide_delay;
	
	string last_import_path;
	ModeType view_mode;
	bool write_metadata = false;

	// Drag and Drop
	enum TargetType {
		UriList,
		TagList,
		PhotoList,
		RootWindow
	};

	private static TargetEntry [] icon_source_target_table = new TargetEntry [] {
		new TargetEntry ("application/x-fspot-photos", 0, (uint) TargetType.PhotoList),
		new TargetEntry ("text/uri-list", 0, (uint) TargetType.UriList),
		new TargetEntry ("application/x-root-window-drop", 0, (uint) TargetType.RootWindow)
	};

	private static TargetEntry [] icon_dest_target_table = new TargetEntry [] {
		new TargetEntry ("application/x-fspot-tags", 0, (uint) TargetType.TagList),
		new TargetEntry ("text/uri-list", 0, (uint) TargetType.UriList),
	};

	private static TargetEntry [] tag_target_table = new TargetEntry [] {
		new TargetEntry ("application/x-fspot-tags", 0, (uint) TargetType.TagList),
	};

	private static TargetEntry [] tag_dest_target_table = new TargetEntry [] {
		new TargetEntry ("application/x-fspot-photos", 0, (uint) TargetType.PhotoList),
		new TargetEntry ("text/uri-list", 0, (uint) TargetType.UriList),
		new TargetEntry ("application/x-fspot-tags", 0, (uint) TargetType.TagList),
	};

	const int PHOTO_IDX_NONE = -1;

	//
	// Constructor
	//
	public MainWindow (Db db)
	{
		this.db = db;

		Glade.XML gui = Glade.XML.FromAssembly ("f-spot.glade", "main_window", "f-spot");
		gui.Autoconnect (this);

		LoadPreference (Preferences.MAIN_WINDOW_WIDTH);
		LoadPreference (Preferences.MAIN_WINDOW_X);
		LoadPreference (Preferences.MAIN_WINDOW_MAXIMIZED);
		LoadPreference (Preferences.SIDEBAR_POSITION);

		slide_delay = new FSpot.Delay (new GLib.IdleHandler (SlideShow));

		toolbar = new Gtk.Toolbar ();
		toolbar_vbox.PackStart (toolbar);
		GtkUtil.MakeToolbarButton (toolbar, "f-spot-rotate-270", new System.EventHandler (HandleRotate270Command));
		GtkUtil.MakeToolbarButton (toolbar, "f-spot-rotate-90", new System.EventHandler (HandleRotate90Command));
		toolbar.AppendSpace ();
		browse_button = GtkUtil.MakeToolbarToggleButton (toolbar, "f-spot-browse", 
								 new System.EventHandler (HandleToggleViewBrowse)) as ToggleButton;
		view_button = GtkUtil.MakeToolbarToggleButton (toolbar, "f-spot-edit-image", 
							       new System.EventHandler (HandleToggleViewPhoto)) as ToggleButton;
		toolbar.AppendSpace ();
		GtkUtil.MakeToolbarButton (toolbar, "f-spot-fullscreen", new System.EventHandler (HandleViewFullscreen));
		GtkUtil.MakeToolbarButton (toolbar, "f-spot-slideshow", new System.EventHandler (HandleViewSlideShow));

		tag_selection_widget = new TagSelectionWidget (db.Tags);
		tag_selection_scrolled.Add (tag_selection_widget);
		
		tag_selection_widget.Selection.Changed += HandleTagSelectionChanged;
		tag_selection_widget.SelectionChanged += OnTagSelectionChanged;
		tag_selection_widget.DragDataGet += HandleTagSelectionDragDataGet;
		tag_selection_widget.DragDrop += HandleTagSelectionDragDrop;
		tag_selection_widget.DragBegin += HandleTagSelectionDragBegin;
		tag_selection_widget.KeyPressEvent += HandleTagSelectionKeyPress;
		Gtk.Drag.SourceSet (tag_selection_widget, Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask,
				    tag_target_table, DragAction.Copy | DragAction.Move);

		tag_selection_widget.DragDataReceived += HandleTagSelectionDragDataReceived;
		tag_selection_widget.DragMotion += HandleTagSelectionDragMotion;
		Gtk.Drag.DestSet (tag_selection_widget, DestDefaults.All, tag_dest_target_table, 
				  DragAction.Copy | DragAction.Move ); 

		tag_selection_widget.ButtonPressEvent += HandleTagSelectionButtonPressEvent;

		info_box = new InfoBox ();
		info_box.VersionIdChanged += HandleInfoBoxVersionIdChange;
		left_vbox.PackStart (info_box, false, true, 0);

		query = new FSpot.PhotoQuery (db.Photos);
		query.ItemChanged += HandleQueryItemChanged;

#if SHOW_CALENDAR
		FSpot.SimpleCalendar cal = new FSpot.SimpleCalendar (query);
		cal.DaySelected += HandleCalendarDaySelected;
		left_vbox.PackStart (cal, false, true, 0);
#endif

		group_selector = new FSpot.GroupSelector ();
		FSpot.GroupAdaptor adaptor = new FSpot.TimeAdaptor (query);

		group_selector.Adaptor  = adaptor;
		group_selector.ShowAll ();

		view_vbox.PackStart (group_selector, false, false, 0);
		view_vbox.ReorderChild (group_selector, 0);

		FSpot.QueryDisplay query_display = new FSpot.QueryDisplay (query, tag_selection_widget);
		view_vbox.PackStart (query_display, false, false, 0);
		view_vbox.ReorderChild (query_display, 1);

		icon_view = new QueryView (query);
		LoadPreference (Preferences.THUMBNAIL_WIDTH);
		LoadPreference (Preferences.SHOW_TAGS);
		LoadPreference (Preferences.SHOW_DATES);
		icon_view_scrolled.Add (icon_view);
		icon_view.Selection.Changed += HandleSelectionChanged;
		icon_view.DoubleClicked += HandleDoubleClicked;
		icon_view.Vadjustment.ValueChanged += HandleIconViewScroll;
		icon_view.GrabFocus ();

		new FSpot.PreviewPopup (icon_view);

		Gtk.Drag.SourceSet (icon_view, Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask,
				    icon_source_target_table, DragAction.Copy | DragAction.Move);
		
		icon_view.DragBegin += HandleIconViewDragBegin;
		icon_view.DragDataGet += HandleIconViewDragDataGet;

		TagMenu menu = new TagMenu (attach_tag, db.Tags);
		menu.TagSelected += HandleAttachTagMenuSelected;

		if (zoom_scale != null) {
			zoom_scale.ValueChanged += HandleZoomScaleValueChanged;
		}
		
		near_image.SetFromStock ("f-spot-stock_near", IconSize.SmallToolbar);
		far_image.SetFromStock ("f-spot-stock_far", IconSize.SmallToolbar);

		menu = new TagMenu (find_tag, db.Tags);
		menu.TagSelected += HandleFindTagMenuSelected;

		PhotoTagMenu pmenu = new PhotoTagMenu ();
		pmenu.TagSelected += HandleRemoveTagMenuSelected;
		remove_tag.Submenu = pmenu;
		
		Gtk.Drag.DestSet (icon_view, DestDefaults.All, icon_dest_target_table, 
				  DragAction.Copy | DragAction.Move); 

		//		icon_view.DragLeave += new DragLeaveHandler (HandleIconViewDragLeave);
		icon_view.DragMotion += HandleIconViewDragMotion;
		icon_view.DragDrop += HandleIconViewDragDrop;
		icon_view.DragDataReceived += HandleIconViewDragDataReceived;
		icon_view.KeyPressEvent += HandleIconViewKeyPressEvent;

		photo_view = new PhotoView (query, db.Photos);
		photo_box.Add (photo_view);
		photo_view.PhotoChanged += HandlePhotoViewPhotoChanged;
		photo_view.ButtonPressEvent += HandlePhotoViewButtonPressEvent;
		photo_view.KeyPressEvent += HandlePhotoViewKeyPressEvent;
		photo_view.UpdateStarted += HandlePhotoViewUpdateStarted;
		photo_view.UpdateFinished += HandlePhotoViewUpdateFinished;

		photo_view.View.ZoomChanged += HandleZoomChanged;

		// Tag typing: focus the tag entry if the user starts typing a tag
		icon_view.KeyPressEvent += HandlePossibleTagTyping;
		photo_view.KeyPressEvent += HandlePossibleTagTyping;
		tag_entry.KeyPressEvent += HandleTagEntryKeyPressEvent;
		tag_entry.FocusOutEvent += HandleTagEntryFocusOutEvent;
		tag_entry.Changed += HandleTagEntryChanged;

		Gtk.Drag.DestSet (photo_view, DestDefaults.All, tag_target_table, 
				  DragAction.Copy | DragAction.Move); 

		photo_view.DragMotion += HandlePhotoViewDragMotion;
		photo_view.DragDrop += HandlePhotoViewDragDrop;
		photo_view.DragDataReceived += HandlePhotoViewDragDataReceived;

		view_notebook.SwitchPage += HandleViewNotebookSwitchPage;
		adaptor.GlassSet += HandleAdaptorGlassSet;

		UpdateMenus ();
		main_window.ShowAll ();

		tagbar.Hide ();

		LoadPreference (Preferences.SHOW_TOOLBAR);
		LoadPreference (Preferences.SHOW_SIDEBAR);
		LoadPreference (Preferences.SHOW_TIMELINE);
		
		Preferences.SettingChanged += OnPreferencesChanged;

		main_window.DeleteEvent += HandleDeleteEvent;

		query_display.HandleChanged (query);

		if (Toplevel == null)
			Toplevel = this;

		// When the icon_view is loaded, set it's initial scroll position
		icon_view.SizeAllocated += HandleIconViewReady;

		UpdateToolbar ();
	}

	// Index into the PhotoQuery.  If -1, no photo is selected or multiple photos are selected.
	private int ActiveIndex () 
	{
		if (view_mode == ModeType.IconView && icon_view.CurrentIdx != -1)
			return icon_view.CurrentIdx;

	        int [] selection = SelectedIds ();
		if (selection.Length == 1) 
			return selection [0];
		else 
			return PHOTO_IDX_NONE;
	}

	public bool PhotoSelectionActive ()
	{
		return SelectedIds().Length > 0;
	}

	private Photo CurrentPhoto {
		get {
			int active = ActiveIndex ();
			if (active >= 0)
				return query.Photos [active];
			else
				return null;
		}
	}

	public Db Database {
		get {
			return db;
		}
	}

	public ModeType ViewMode {
		get {
			return view_mode;
		}
	}

	// Switching mode.
	public enum ModeType {
		IconView,
		PhotoView
	};

	public void SetViewMode (ModeType value)
	{
		if (view_mode == value)
			return;

		view_mode = value;
		switch (view_mode) {
		case ModeType.IconView:
			if (view_notebook.CurrentPage != 0)
				view_notebook.CurrentPage = 0;
				
			JumpTo (photo_view.Item.Index);
			zoom_scale.Value = icon_view.ThumbnailWidth / 256.0;
			break;
		case ModeType.PhotoView:
			if (view_notebook.CurrentPage != 1)
				view_notebook.CurrentPage = 1;
			
			JumpTo (icon_view.FocusCell);
			zoom_scale.Value = photo_view.Zoom;
			break;
		}
		UpdateToolbar ();
	}
	
	void UpdateToolbar ()
	{
		if (browse_button != null) {
			bool state = view_mode == ModeType.IconView;
			
			if (browse_button.Active != state)
				browse_button.Active = state;
		}

		if (view_button != null) {
			bool state = view_mode == ModeType.PhotoView;
			
			if (view_button.Active != state)
				view_button.Active = state;
		}
	}

	void HandleViewNotebookSwitchPage (object sender, SwitchPageArgs args)
	{
		switch (view_notebook.CurrentPage) {
		case 0:
			SetViewMode (ModeType.IconView);
			break;
		case 1:
			SetViewMode (ModeType.PhotoView);
			break;
		}
	}

	private int lastTopLeftCell = -1;

	public int [] SelectedIds () {
		int [] ids = new int [0];

		if (fsview != null)
			ids = new int [] { fsview.View.Item.Index };
		else {
			switch (view_mode) {
			case ModeType.IconView:
				ids = icon_view.Selection.Ids;
				break;
			default:
			case ModeType.PhotoView:
				if (photo_view.Item.IsValid)
					ids = new int [] { photo_view.Item.Index };
				break;
			}
		}

		return ids;
	}

	public class Selection : IBrowsableCollection {
		MainWindow win;

		public Selection (MainWindow win)
		{
			this.win = win;
		}
		
		public int Count {
			get {
				switch (win.view_mode) {
				case ModeType.PhotoView:
					return win.photo_view.Item.IsValid ? 1 : 0;
				case ModeType.IconView:
					return win.icon_view.Selection.Count;
				}
				return 0;
			}
		}

		public int IndexOf (IBrowsableItem item)
		{
			switch (win.view_mode) {
			case ModeType.PhotoView:
				return item == win.photo_view.Item.Current ? 0 : -1;
			case ModeType.IconView:
				return win.icon_view.Selection.IndexOf (item);
			}
			return -1;
		}
		
		public bool Contains (IBrowsableItem item)
		{
			switch (win.view_mode) {
			case ModeType.PhotoView:
				return item == win.photo_view.Item.Current ? true : false;
			case ModeType.IconView:
				return win.icon_view.Selection.Contains (item);
			}
			return false;
		}
		
		public IBrowsableItem this [int index] {
			get {
				switch (win.view_mode) {
				case ModeType.PhotoView:
					if (index == 0)
						return win.photo_view.Item.Current;
					break;
				case ModeType.IconView:
					return win.icon_view.Selection [index];
				}
				throw new ArgumentOutOfRangeException ();
			}
		}
		 
		public IBrowsableItem [] Items {
			get {
				switch (win.view_mode) {
				case ModeType.PhotoView:
					if (win.photo_view.Item.IsValid)
						return new IBrowsableItem [] {win.photo_view.Item.Current};

					break;
				case ModeType.IconView:
					return win.icon_view.Selection.Items;
				}
				return new IBrowsableItem [0];
			}
		}

		private void HandleSelectionChanged (IBrowsableCollection collection)
		{
			if (Changed != null)
				Changed (this);
		}

		private void HandleSelectionItemChanged (IBrowsableCollection collection, int item)
		{
			if (ItemChanged != null)
				ItemChanged (this, item);
		}

		public event IBrowsableCollectionChangedHandler Changed;
		public event IBrowsableCollectionItemChangedHandler ItemChanged;
	}
	//
	// Selection Interface
	//

	private Photo [] SelectedPhotos (int [] selected_ids)
	{
		Photo [] photo_list = new Photo [selected_ids.Length];
	
		int i = 0;
		foreach (int num in selected_ids)
			photo_list [i ++] = query.Photos [num];
		
		return photo_list;
	}

	private Photo [] SelectedPhotos () 
	{
		return SelectedPhotos (SelectedIds ());
	}

	//
	// Change Notification functions
	//

	private void InvalidateViews ()
	{
		icon_view.QueueDraw ();
		photo_view.Reload ();
		if (fsview != null)
			fsview.View.Reload ();
	}
		
	//
	// Commands
	//

	private void RotateSelectedPictures (RotateCommand.Direction direction)
	{
		RotateCommand command = new RotateCommand (main_window);
		
		int [] selected_ids = SelectedIds ();
		if (command.Execute (direction, SelectedPhotos (selected_ids))) {
			foreach (int num in selected_ids)
				query.MarkChanged (num);
		}
	}

	//
	// Tag Selection Drag Handlers
	//

	public void AddTagExtended (int num, Tag [] tags)
	{
		Photo p = query.Photos [num];

		p.AddTag (tags);

		if (write_metadata)
			p.WriteMetadataToImage ();

		query.Commit (num);

		foreach (Tag t in tags) {
			Pixbuf icon = null;

			if (t.Icon == null) {
				if (icon == null) {
					// FIXME this needs a lot more work.
					try {
						Pixbuf tmp = FSpot.PhotoLoader.LoadAtMaxSize (query.Items [num], 128, 128);
						icon = PixbufUtils.TagIconFromPixbuf (tmp);
						tmp.Dispose ();
					} catch {
						icon = null;
					}
				}
				
				t.Icon = icon;
				db.Tags.Commit (t);
			}
		}
	}

	[GLib.ConnectBefore]
	void HandleTagSelectionButtonPressEvent (object sender, ButtonPressEventArgs args)
	{
		if (args.Event.Button == 3)
		{
			TagPopup popup = new TagPopup ();
			popup.Activate (args.Event, tag_selection_widget.TagAtPosition ((int)args.Event.X, (int)args.Event.Y),
					tag_selection_widget.TagHighlight ());
			args.RetVal = true;
		}
	}

	void HandleTagSelectionDragBegin (object sender, DragBeginArgs args)
	{
		Tag [] tags = tag_selection_widget.TagHighlight ();
		int len = tags.Length;
		int size = 32;
		int csize = size/2 + len * size / 2 + 2;
		
		Pixbuf container = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, csize, csize);
		container.Fill (0x00000000);
		
		bool use_icon = false;;
		while (len-- > 0) {
			Pixbuf thumbnail = tags[len].Icon;
			
			if (thumbnail != null) {
				Pixbuf small = PixbufUtils.ScaleToMaxSize (thumbnail, size, size);				
				
				int x = len * (size/2) + (size - small.Width)/2;
				int y = len * (size/2) + (size - small.Height)/2;

				small.Composite (container, x, y, small.Width, small.Height, x, y, 1.0, 1.0, Gdk.InterpType.Nearest, 0xff);
				small.Dispose ();

				use_icon = true;
			}
		}
		if (use_icon)
			Gtk.Drag.SetIconPixbuf (args.Context, container, 0, 0);
		container.Dispose ();
	}
	
	void HandleTagSelectionDragDataGet (object sender, DragDataGetArgs args)
	{		
		UriList list = new UriList (SelectedPhotos ());

		switch (args.Info) {
		case (uint) TargetType.TagList:
			Byte [] data = Encoding.UTF8.GetBytes (list.ToString ());
			Atom [] targets = args.Context.Targets;
		
			args.SelectionData.Set (targets[0], 8, data, data.Length);
			break;
		} 
	}

	void HandleTagSelectionDragDrop (object sender, DragDropArgs args)
	{
		args.RetVal = true;
	}

	public void HandleTagSelectionDragMotion (object o, DragMotionArgs args)
	{
		TreePath path;

		if (!tag_selection_widget.GetPathAtPos (args.X, args.Y, out path))
			return;

		tag_selection_widget.SetDragDestRow (path, Gtk.TreeViewDropPosition.IntoOrAfter);
	}

	public void HandleTagSelectionDragDataReceived (object o, DragDataReceivedArgs args)
	{
		Tag [] tags = new Tag [1];

		//FIXME this is a lame api, we need to fix the drop behaviour of these things
		tags [0] = tag_selection_widget.TagAtPosition(args.X, args.Y);

		if (tags [0] == null)
			return;

		switch (args.Info) {
		case (uint)TargetType.PhotoList:
			foreach (int num in SelectedIds ()) {
				AddTagExtended (num, tags);
			}
			break;
		case (uint)TargetType.UriList:
			UriList list = new UriList (args.SelectionData);
			
			foreach (string path in list.ToLocalPaths ()) {
				Photo photo = db.Photos.GetByPath (path);
				
				// FIXME - at this point we should import the photo, and then continue
				if (photo == null)
					return;
				
				// FIXME this should really follow the AddTagsExtended path too
				photo.AddTag (tags);
			}
			InvalidateViews ();
			break;
		case (uint)TargetType.TagList:
			Tag child = tag_selection_widget.TagHighlight ()[0];
			Tag parent = tags[0];

			// FIXME with this reparenting via dnd, you cannot move a tag to root.
			if (child != parent && !child.IsAncestorOf(parent) && child.Category != parent && parent is Category)
			{
				child.Category = parent as Category;

				// Saving changes will automatically cause the TreeView to be updated
				db.Tags.Commit (child);
				
				args.RetVal = true;
			} else {
				args.RetVal = false;
			}

			break;
		}

		UpdateTagEntryFromSelection ();
	}

#if SHOW_CALENDAR
	void HandleCalendarDaySelected (object sender, System.EventArgs args)
	{
		FSpot.SimpleCalendar cal = sender as FSpot.SimpleCalendar;
		JumpTo (cal.Date);
	}
#endif

	private void JumpTo (System.DateTime time)
	{
		//FIXME this should make sure the photos are sorted by
		//time.  This should be handled via a property that
		//does all the needed switching.
		if (!(group_selector.Adaptor is FSpot.TimeAdaptor))
			HandleArrangeByTime (null, null);
		
		FSpot.TimeAdaptor time_adaptor = group_selector.Adaptor as FSpot.TimeAdaptor;
		if (time_adaptor != null)
			JumpTo (time_adaptor.LookupItem (time));
	}

	private void JumpTo (int index)
	{
		switch (view_mode) {
		case ModeType.PhotoView:
			photo_view.Item.Index = index;
			break;
		case ModeType.IconView:
			icon_view.ScrollTo (index);
			icon_view.Throb (index);
			break;
		}
	}

	void HandleAdaptorGlassSet (FSpot.GroupAdaptor sender, int index)
	{
		JumpTo (index);
	}

	/*
	 * Keep the glass temporal slider in sync with the user's scrolling in the icon_view
	 */
	private void UpdateGlass ()
	{
		int cell_num = icon_view.TopLeftVisibleCell();
		
		if (cell_num == -1 || cell_num == lastTopLeftCell)
			return;

		lastTopLeftCell = cell_num;
		FSpot.IBrowsableItem photo = icon_view.Collection.Items [cell_num];
#if false
		group_selector.Adaptor.GlassSet -= HandleAdaptorGlassSet;
		group_selector.Adaptor.SetGlass (group_selector.Adaptor.IndexFromPhoto (photo));
		group_selector.Adaptor.GlassSet = HandleAdaptorGlassSet;
#else
		/* 
		 * FIXME this is a lame hack to get around a delagate chain.  This should 
		 * actually operate directly on the adaptor not on the selector but I don't have 
		 * time to fix it right now.
		 */
		group_selector.SetPosition (group_selector.Adaptor.IndexFromPhoto (photo));
#endif
	}
	
	void HandleIconViewScroll (object sender, EventArgs args)
	{
		UpdateGlass ();
	}

	void HandleIconViewReady (object sender, EventArgs args)
	{
		LoadPreference (Preferences.ICON_VIEW_POSITION);

		// We only want to set the position the first time
		// the icon_view is ready (eg on startup)
		icon_view.SizeAllocated -= HandleIconViewReady;
	}

	//
	// IconView Drag Handlers
	//

	void HandleIconViewDragBegin (object sender, DragBeginArgs args)
	{
		Photo [] photos = SelectedPhotos ();
		
		if (photos.Length > 0) {
			int len = Math.Min (photos.Length, 4);
			int size = 48;
			int border  = 2;
			int csize = size/2 + len * size / 2 + 2 * border ;
			
			Pixbuf container = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, csize, csize);
			container.Fill (0x00000000);

			bool use_icon = false;;
			while (len-- > 0) {
				string thumbnail_path = FSpot.ThumbnailGenerator.ThumbnailPath (photos [len].DefaultVersionUri);
				FSpot.PixbufCache.CacheEntry entry = icon_view.Cache.Lookup (thumbnail_path);

				Pixbuf thumbnail = null;
				if (entry != null)
					thumbnail = entry.ShallowCopyPixbuf ();
				
				if (thumbnail != null) {
					Pixbuf small = PixbufUtils.ScaleToMaxSize (thumbnail, size, size);				

					int x = border + len * (size/2) + (size - small.Width)/2;
					int y = border + len * (size/2) + (size - small.Height)/2;
					Pixbuf box = new Pixbuf (container, x - border, y - border, 
								 small.Width + 2 * border, small.Height + 2 * border);

					box.Fill (0x000000ff);
					small.CopyArea (0, 0, small.Width, small.Height, container, x, y); 
					
					thumbnail.Dispose ();
					small.Dispose ();
					use_icon = true;
				}
			}
			if (use_icon)
				Gtk.Drag.SetIconPixbuf (args.Context, container, 0, 0);
			container.Dispose ();
		}
	}

	void HandleIconViewDragDataGet (object sender, DragDataGetArgs args)
	{		
		switch (args.Info) {
		case (uint) TargetType.UriList:
		case (uint) TargetType.PhotoList:
			UriList list = new UriList (SelectedPhotos ());
			Byte [] data = Encoding.UTF8.GetBytes (list.ToString ());
			Atom [] targets = args.Context.Targets;
			args.SelectionData.Set (targets[0], 8, data, data.Length);
			break;
		case (uint) TargetType.RootWindow:
			HandleSetAsBackgroundCommand (null, null);
                        break;
		}
		       
	}

	void HandleIconViewDragDrop (object sender, DragDropArgs args)
	{
		//Widget source = Gtk.Drag.GetSourceWidget (args.Context);
		//Console.WriteLine ("Drag Drop {0}", source == null ? "null" : source.TypeName);
		
		args.RetVal = true;
	}

	void HandleIconViewDragMotion (object sender, DragMotionArgs args)
	{
		//Widget source = Gtk.Drag.GetSourceWidget (args.Context);
		//Console.WriteLine ("Drag Motion {0}", source == null ? "null" : source.TypeName);

		Gdk.Drag.Status (args.Context, args.Context.SuggestedAction, args.Time);
		args.RetVal = true;
	}

	void HandleIconViewKeyPressEvent (object sender, Gtk.KeyPressEventArgs args)
	{
		if (args.Event.Key == Gdk.Key.Delete) {
			HandleRemoveCommand (sender, (EventArgs) args);
		}
	}

	public void ImportUriList (UriList list) 
	{
		ImportCommand command = new ImportCommand (main_window);
		if (command.ImportFromPaths (db.Photos, list.ToLocalPaths ()) > 0) {
			UpdateQuery ();
		}
	}

	public void ImportFile (string path)
	{
		ImportCommand command = new ImportCommand (main_window);
		if (command.ImportFromFile (db.Photos, path) > 0) {
			UpdateQuery ();
		}
	}

	void HandleIconViewDragDataReceived (object sender, DragDataReceivedArgs args)
	{
	 	Widget source = Gtk.Drag.GetSourceWidget (args.Context);     
		
		switch (args.Info) {
		case (uint)TargetType.TagList:
			//
			// Translate the event args from viewport space to window space,
			// drag events use the viewport.  Owen sends his regrets.
			//
			int item = icon_view.CellAtPosition (args.X + (int) icon_view.Hadjustment.Value, 
							     args.Y + (int) icon_view.Vadjustment.Value);

			//Console.WriteLine ("Drop cell = {0} ({1},{2})", item, args.X, args.Y);
			if (item >= 0) {
				if (icon_view.Selection.Contains (item))
					AttachTags (tag_selection_widget.TagHighlight (), SelectedIds());
				else 
					AttachTags (tag_selection_widget.TagHighlight (), new int [] {item});

				UpdateTagEntryFromSelection ();
			}
			break;
		case (uint)TargetType.UriList:

			/* 
			 * If the drop is coming from inside f-spot then we don't want to import 
			 */
			if (source != null)
				return;

			UriList list = new UriList (args.SelectionData);
			ImportUriList (list);
			break;
		}

		Gtk.Drag.Finish (args.Context, true, false, args.Time);
	}

	//
	// IconView event handlers
	// 

	void HandleSelectionChanged (FSpot.IBrowsableCollection collection)
	{
		info_box.Photo = CurrentPhoto;
		if (info_display != null)
			info_display.Photo = CurrentPhoto;

		UpdateMenus ();
		UpdateTagEntryFromSelection ();
	}

	void HandleDoubleClicked (IconView icon_view, int clicked_item)
	{
		icon_view.FocusCell = clicked_item;
		SetViewMode (ModeType.PhotoView);
	}

	//
	// PhotoView event handlers.
	//

	void HandlePhotoViewPhotoChanged (PhotoView sender)
	{
		info_box.Photo = CurrentPhoto;
		if (info_display != null)
			info_display.Photo = CurrentPhoto;
		UpdateMenus ();
		UpdateTagEntryFromSelection ();
	}
	
	void HandlePhotoViewKeyPressEvent (object sender, Gtk.KeyPressEventArgs args)
	{
		switch (args.Event.Key) {
		case Gdk.Key.F:
		case Gdk.Key.f:
			HandleViewFullscreen (sender, args);
			args.RetVal = true;
			break;
		case Gdk.Key.Escape:
			SetViewMode (ModeType.IconView);
			args.RetVal = true;
			break;
		case Gdk.Key.Delete:
			HandleRemoveCommand (sender, (EventArgs) args);
			args.RetVal = true;
			break;
		default:
			break;
		}
		return;
	}

	void HandlePhotoViewButtonPressEvent (object sender, Gtk.ButtonPressEventArgs args)
	{
		if (args.Event.Type == EventType.TwoButtonPress && args.Event.Button == 1)
			SetViewMode (ModeType.IconView);
	}

	void HandlePhotoViewUpdateStarted (PhotoView sender)
	{
		main_window.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
		// FIXME: use gdk_display_flush() when available
		main_window.GdkWindow.Display.Sync ();
	}

	void HandlePhotoViewUpdateFinished (PhotoView sender)
	{
		main_window.GdkWindow.Cursor = null;
		// FIXME: use gdk_display_flush() when available
		main_window.GdkWindow.Display.Sync ();
	}

	//
	// PhotoView drag handlers.
	//

	void HandlePhotoViewDragDrop (object sender, DragDropArgs args)
	{
		//Widget source = Gtk.Drag.GetSourceWidget (args.Context);
		//Console.WriteLine ("Drag Drop {0}", source == null ? "null" : source.TypeName);

		args.RetVal = true;
	}

	void HandlePhotoViewDragMotion (object sender, DragMotionArgs args)
	{
		//Widget source = Gtk.Drag.GetSourceWidget (args.Context);
		//Console.WriteLine ("Drag Motion {0}", source == null ? "null" : source.TypeName);

		Gdk.Drag.Status (args.Context, args.Context.SuggestedAction, args.Time);
		args.RetVal = true;
	}

	void HandlePhotoViewDragDataReceived (object sender, DragDataReceivedArgs args)
	{
	 	//Widget source = Gtk.Drag.GetSourceWidget (args.Context);     
		//Console.WriteLine ("Drag received {0}", source == null ? "null" : source.TypeName);

		HandleAttachTagCommand (sender, null);
		
		Gtk.Drag.Finish (args.Context, true, false, args.Time);
	}	

	//
	// TagMenu commands.
	//

	public void HandleTagMenuActivate (object sender, EventArgs args)
	{

		MenuItem parent = sender as MenuItem;
		if (parent != null && parent.Submenu is PhotoTagMenu) {
			PhotoTagMenu menu = (PhotoTagMenu) parent.Submenu;
			menu.Populate (SelectedPhotos ()); 
		}
	}

	public void HandleAttachTagMenuSelected (Tag t) 
	{
		foreach (int num in SelectedIds ()) {
			AddTagExtended (num, new Tag [] {t});
		}

		UpdateTagEntryFromSelection ();
	}
	
	void HandleFindTagMenuSelected (Tag t)
	{
		tag_selection_widget.TagSelection = new Tag [] {t};
	}

	public void HandleRemoveTagMenuSelected (Tag t)
	{
		foreach (int num in SelectedIds ()) {
			query.Photos [num].RemoveTag (t);
			query.Commit (num);
		}

		UpdateTagEntryFromSelection ();
	}

	//
	// Main menu commands
	//

	void HandleOpenCommand (object sender, EventArgs e)
	{
		new FSpot.SingleView ();
	}

	void HandleImportCommand (object sender, EventArgs e)
	{
		db.Sync = false;
		ImportCommand command = new ImportCommand (main_window);
		if (command.ImportFromFile (db.Photos, this.last_import_path) > 0) {
			this.last_import_path = command.ImportPath;
			UpdateQuery ();
		}
		db.Sync = true;		
	}

	void HandleImportFromCameraCommand (object sender, EventArgs e)
	{
		ImportCamera (null);
	}

	public void ImportCamera (string camera_device)
	{
		GPhotoCamera cam = new GPhotoCamera();

		try {
			int num_cameras = cam.DetectCameras();
			int selected_cam = 0;

			if (num_cameras < 1) {
				HigMessageDialog md = new HigMessageDialog (main_window, DialogFlags.DestroyWithParent, 
					MessageType.Warning, ButtonsType.Ok, 
					Mono.Posix.Catalog.GetString ("No cameras detected."),
					Mono.Posix.Catalog.GetString ("F-Spot was unable to find any cameras attached to this system." + 
								      "  Double check that the camera is connected and has power")); 

				md.Run ();
				md.Destroy ();
				return;
			} else if (num_cameras == 1) {
				selected_cam = 0;
			} else {
				bool found = false;
				if (camera_device != null) {
					string port = camera_device.Remove (0, "gphoto2:".Length);
					for (int i = 0; i < num_cameras; i++)
						if (cam.CameraList.GetValue (i) == port) {
							selected_cam = i;
							found = true;
							break;
						}
				}
				
				if (!found) {
					FSpot.CameraSelectionDialog camselect = new FSpot.CameraSelectionDialog (cam.CameraList);
					selected_cam = camselect.Run ();
				}
			}

			if (selected_cam >= 0) {
				cam.SelectCamera (selected_cam);	
				cam.InitializeCamera ();

				FSpot.CameraFileSelectionDialog selector = new FSpot.CameraFileSelectionDialog (cam, db);
				selector.Run ();

				UpdateQuery ();
			}
		}
		catch (GPhotoException ge) {
			System.Console.WriteLine (ge.ToString ());
			HigMessageDialog md = new HigMessageDialog (main_window, DialogFlags.DestroyWithParent, 
				MessageType.Error, ButtonsType.Ok, 
				Mono.Posix.Catalog.GetString ("Error connecting to camera"),
				String.Format (Mono.Posix.Catalog.GetString ("Received error \"{0}\" while connecting to camera"), 
				ge.Message));

			md.Run ();
			md.Destroy ();
		} finally {
			cam.ReleaseGPhotoResources ();
		}
	}
	
	unsafe void HandlePrintCommand (object sender, EventArgs e)
	{
		new FSpot.PrintDialog (SelectedPhotos ());
	}

	private Gtk.Dialog info_display_window;
	public void HandleInfoDisplayDestroy (object sender, EventArgs args)
	{
		info_display_window = null;
		info_display = null;
	}
	
	void HandleViewFullExif (object sender, EventArgs args)
	{
		if (info_display_window != null) {
			info_display_window.Present ();
			return;
		}

		info_display = new FSpot.InfoDisplay ();
		info_display_window = new Gtk.Dialog ("Metadata Browser", main_window, 
						      Gtk.DialogFlags.NoSeparator | Gtk.DialogFlags.DestroyWithParent);
		info_display_window.SetDefaultSize (400, 400);
		Gtk.ScrolledWindow scroll = new ScrolledWindow ();
		info_display_window.VBox.PackStart (scroll);
		scroll.Add (info_display);

		info_display.Photo = CurrentPhoto;
	       
		info_display_window.ShowAll ();
		info_display_window.Destroyed += HandleInfoDisplayDestroy;
	}


	void HandleExportToGallery (object sender, EventArgs args)
	{
		new FSpot.GalleryExport (new FSpot.PhotoArray (SelectedPhotos ()));
	}

	void HandleExportToOriginal (object sender, EventArgs args)
	{
		new FSpot.FolderExport (new FSpot.PhotoArray (SelectedPhotos ()));
	}

	void HandleViewDirectory (object sender, EventArgs args)
	{
		Gtk.Window win = new Gtk.Window ("Directory View");
		IconView view = new IconView (new FSpot.DirectoryCollection (System.IO.Directory.GetCurrentDirectory ()));
		new FSpot.PreviewPopup (view);

		view.DisplayTags = false;

		Gtk.ScrolledWindow scrolled = new ScrolledWindow ();
		win.Add (scrolled);
		scrolled.Add (view);
		win.ShowAll ();
	}

	void HandleViewSelection (object sender, EventArgs args)
	{
		Gtk.Window win = new Gtk.Window ("This is a window");
		Gtk.ScrolledWindow scroll = new Gtk.ScrolledWindow ();
	
		win.Add (scroll);
		scroll.Add (new TrayView (icon_view.Selection));
		win.ShowAll ();
	}

	void HandleExportToFlickr (object sender, EventArgs args)
	{
		new FSpot.FlickrExport (new FSpot.PhotoArray (SelectedPhotos ()));
	}
	
	void HandleExportToFotki (object sender, EventArgs args)
	{
		
	}
	
	void HandleExportToCD (object sender, EventArgs args)
	{
		new FSpot.CDExport (new FSpot.PhotoArray (SelectedPhotos ()));
	}

	void HandleSendMailCommand (object sender, EventArgs args)
	{
		StringBuilder url = new StringBuilder ("mailto:?subject=my%20photos");

		foreach (Photo p in SelectedPhotos ()) {
			url.Append ("&attach=" + p.DefaultVersionPath);
		}

		GnomeUtil.UrlShow (main_window, url.ToString ());
	}

	void HandleAbout (object sender, EventArgs args)
	{
		string [] authors = new string [] {
			"Ettore Perazzoli",
			"Lawrence Ewing",
			"Laurence Hygate",
			"Nat Friedman",
			"Miguel de Icaza",
			"Todd Berman",
			"Vladimir Vukicevic",
			"Aaron Bockover",
			"Jon Trowbridge",
			"Joe Shaw",
			"Tambet Ingo",
			"Bengt Thuree",
			"MOREAU Vincent",
			"Alvaro del Castillo",
			"Lee Willis",
			"Alessandro Gervaso",
			"Peter Johanson",
			"Grahm Orr",
			"Ewen Cheslack-Postava",
			"Gabriel Burt",
			"Patanjali Somayaji",
			"Matt Jones",
			"Martin Willemoes Hansen",
			"Joshua Tauberer",
			"Joerg Buesse",
			"Jakub Steiner",
			"Xavier Bouchoux"
		};

                // Translators should localize the following string
                // * which will give them credit in the About box.
                // * E.g. "Martin Willemoes Hansen"
                string translators = Mono.Posix.Catalog.GetString ("translator-credits");
                if(System.String.Compare(translators,"translator-credits") == 0) {
                    translators = null;
                }

                new About ("F-Spot", 
			   FSpot.Defines.VERSION, 
			   "Copyright 2003-2005 Novell Inc.",
                           null, authors, new string [0], translators, null).Show();
	}

	void HandleArrangeByTime (object sender, EventArgs args)
	{
		group_selector.Adaptor.GlassSet -= HandleAdaptorGlassSet;
		FSpot.GroupAdaptor adaptor = new FSpot.TimeAdaptor (query);
		group_selector.Adaptor = adaptor;
		group_selector.Mode = FSpot.GroupSelector.RangeType.Min;
		adaptor.GlassSet += HandleAdaptorGlassSet;
	}

	void HandleArrangeByDirectory (object sender, EventArgs args)
	{
		group_selector.Adaptor.GlassSet -= HandleAdaptorGlassSet;
		FSpot.GroupAdaptor adaptor = new FSpot.DirectoryAdaptor (query);		
		group_selector.Adaptor = adaptor;
		group_selector.Mode = FSpot.GroupSelector.RangeType.Min;
		adaptor.GlassSet += HandleAdaptorGlassSet;
	}

	// Called when the user clicks the X button	
	void HandleDeleteEvent (object sender, DeleteEventArgs args)
	{
		Close();
		args.RetVal = true;
	}

	void HandleCloseCommand (object sender, EventArgs args)
	{
		Close();
	}
	
	void Close ()
	{
		int x, y, width, height;
		main_window.GetPosition (out x, out y);
		main_window.GetSize (out width, out height);

		bool maximized = ((main_window.GdkWindow.State & Gdk.WindowState.Maximized) > 0);
		Preferences.Set (Preferences.MAIN_WINDOW_MAXIMIZED, maximized);

		if (!maximized) {
			Preferences.Set (Preferences.MAIN_WINDOW_X,		x);
			Preferences.Set (Preferences.MAIN_WINDOW_Y,		y);
			Preferences.Set (Preferences.MAIN_WINDOW_WIDTH,		width);
			Preferences.Set (Preferences.MAIN_WINDOW_HEIGHT,	height);
		}

		Preferences.Set (Preferences.SHOW_TOOLBAR,		toolbar.Visible);
		Preferences.Set (Preferences.SHOW_SIDEBAR,		info_vpaned.Visible);
		Preferences.Set (Preferences.SHOW_TIMELINE,		group_selector.Visible);
		Preferences.Set (Preferences.SHOW_TAGS,			icon_view.DisplayTags);
		Preferences.Set (Preferences.SHOW_DATES,		icon_view.DisplayDates);

		Preferences.Set (Preferences.SIDEBAR_POSITION,		main_hpaned.Position);
		Preferences.Set (Preferences.THUMBNAIL_WIDTH,		icon_view.ThumbnailWidth);
	
		Preferences.Set (Preferences.ICON_VIEW_POSITION, icon_view.TopLeftVisibleCell ());

		this.Window.Destroy ();
	}
	
	void HandleCreateVersionCommand (object obj, EventArgs args)
	{
		PhotoVersionCommands.Create cmd = new PhotoVersionCommands.Create ();

		if (cmd.Execute (db.Photos, CurrentPhoto, main_window)) {
			query.MarkChanged (ActiveIndex ());
		}
	}

	void HandleDeleteVersionCommand (object obj, EventArgs args)
	{
		PhotoVersionCommands.Delete cmd = new PhotoVersionCommands.Delete ();

		if (cmd.Execute (db.Photos, CurrentPhoto, main_window)) {
			query.MarkChanged (ActiveIndex ());
		}
	}

	void HandlePropertiesCommand (object obje, EventArgs args)
	{
		Photo [] photos = SelectedPhotos ();
		
	        long length = 0;

		foreach (Photo p in photos) {
			System.IO.FileInfo fi = new System.IO.FileInfo (p.DefaultVersionPath);

			length += fi.Length;
		}

		Console.WriteLine ("{0} Selected Photos : Total length = {1} - {2}kB - {3}MB", photos.Length, length, length / 1024, length / (1024 * 1024));
	}
		
	void HandleRenameVersionCommand (object obj, EventArgs args)
	{
		PhotoVersionCommands.Rename cmd = new PhotoVersionCommands.Rename ();

		if (cmd.Execute (db.Photos, CurrentPhoto, main_window)) {
			query.MarkChanged (ActiveIndex ());
		}
	}

	public void HandleCreateNewTagCommand (object sender, EventArgs args)
	{
		TagCommands.Create command = new TagCommands.Create (db.Tags, main_window);
		command.Execute (TagCommands.TagType.Tag, tag_selection_widget.TagHighlight ());
		
	}

	public void HandleCreateNewCategoryCommand (object sender, EventArgs args)
	{
		TagCommands.Create command = new TagCommands.Create (db.Tags, main_window);
		command.Execute (TagCommands.TagType.Category, tag_selection_widget.TagHighlight ());
	}

	public void HandleAttachTagCommand (object obj, EventArgs args)
	{
		AttachTags (tag_selection_widget.TagHighlight (), SelectedIds ());
	}

	void AttachTags (Tag [] tags, int [] ids) 
	{
		foreach (int num in ids) {
			AddTagExtended (num, tags);
		}
	}

	public void HandleRemoveTagCommand (object obj, EventArgs args)
	{
		Tag [] tags = this.tag_selection_widget.TagHighlight ();

		foreach (int num in SelectedIds ()) {
			query.Photos [num].RemoveTag (tags);
			query.Commit (num);
		}

		UpdateTagEntryFromSelection ();
	}

	public void HandleEditSelectedTag (object sender, EventArgs ea)
	{
		Tag [] tags = this.tag_selection_widget.TagHighlight ();
		if (tags.Length != 1)
			return;

		HandleEditSelectedTagWithTag (tags [0]);
	}

	public void HandleEditSelectedTagWithTag (Tag tag)
	{
		if (tag == null)
			return;
		
		TagCommands.Edit command = new TagCommands.Edit (db, main_window);
		command.Execute (tag);
		if (view_mode == ModeType.IconView)
			icon_view.QueueDraw ();
	}

	public void HandleMergeTagsCommand (object obj, EventArgs args)
	{
		Tag [] tags = this.tag_selection_widget.TagHighlight ();
		if (tags.Length < 2)
			return;

		System.Array.Sort (tags, new TagRemoveComparer ());
		string header = Mono.Posix.Catalog.GetString ("Merge the {0} selected tags?");
		
		header = String.Format (header, tags.Length);
		string msg = Mono.Posix.Catalog.GetString("This operation will delete all but one of the selected tags.");
		string ok_caption = Mono.Posix.Catalog.GetString ("_Merge tags");
		
		if (ResponseType.Ok != HigMessageDialog.RunHigConfirmation(main_window, 
									   DialogFlags.DestroyWithParent, 
									   MessageType.Warning, 
									   header, 
									   msg, 
									   ok_caption))
			return;
		
		// The surviving tag is tags [0].  removetags will contain
		// the tags to be merged.
		Tag [] removetags = new Tag [tags.Length - 1];
		Array.Copy (tags, 1, removetags, 0, tags.Length - 1);


		// Remove the defunct tags from all the photos and
		// replace them with the new tag.
		Photo [] photos = db.Photos.Query (tags);
		foreach (Photo p in photos) {
			p.RemoveTag (removetags);
			p.AddTag (tags [0]);
			db.Photos.Commit (p);
		}

		// Remove the defunct tags from the tag list.
		db.Photos.Remove (removetags);

		UpdateTagEntryFromSelection ();
		icon_view.QueueDraw ();
	}

	void HandleAdjustColor (object sender, EventArgs args)
	{
		if (ActiveIndex () > 0) {
			SetViewMode (ModeType.PhotoView);
			new FSpot.ColorDialog (photo_view.View);
		}
	}

	void HandleSharpen (object sender, EventArgs args)
	{
		Gtk.Dialog dialog = new Gtk.Dialog (Mono.Posix.Catalog.GetString ("Unsharp Mask"), main_window, Gtk.DialogFlags.Modal);

		dialog.VBox.Spacing = 6;
		dialog.VBox.BorderWidth = 12;
		
		Gtk.Table table = new Gtk.Table (3, 2, false);
		table.ColumnSpacing = 6;
		table.RowSpacing = 6;
		
		table.Attach (new Gtk.Label (Mono.Posix.Catalog.GetString ("Amount:")), 0, 1, 0, 1);
		table.Attach (new Gtk.Label (Mono.Posix.Catalog.GetString ("Radius:")), 0, 1, 1, 2);
		table.Attach (new Gtk.Label (Mono.Posix.Catalog.GetString ("Threshold:")), 0, 1, 2, 3);

		Gtk.SpinButton amount_spin = new Gtk.SpinButton (0.5, 100.0, .01);
		Gtk.SpinButton radius_spin = new Gtk.SpinButton (5.0, 50.0, .01);
		Gtk.SpinButton threshold_spin = new Gtk.SpinButton (0.0, 50.0, .01);

		table.Attach (amount_spin, 1, 2, 0, 1);
		table.Attach (radius_spin, 1, 2, 1, 2);
		table.Attach (threshold_spin, 1, 2, 2, 3);
		
		dialog.VBox.PackStart (table);

		dialog.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
		dialog.AddButton (Gtk.Stock.Ok, Gtk.ResponseType.Ok);

		dialog.ShowAll ();

		Gtk.ResponseType response = (Gtk.ResponseType) dialog.Run ();

		if (response == Gtk.ResponseType.Ok) {
			foreach (int id in SelectedIds ()) {
				Gdk.Pixbuf orig = FSpot.PhotoLoader.Load (query, id);
				Gdk.Pixbuf final = PixbufUtils.UnsharpMask (orig, radius_spin.Value, amount_spin.Value, threshold_spin.Value);
			
				Photo photo = query.Photos [id];
				
				bool create_version = photo.DefaultVersionId == Photo.OriginalVersionId;

				try {
					photo.SaveVersion (final, create_version);
				} catch (System.Exception e) {
					string msg = Mono.Posix.Catalog.GetString ("Error saving sharpened photo");
					string desc = String.Format (Mono.Posix.Catalog.GetString ("Received exception \"{0}\". Unable to save image {1}"),
								     e.Message, photo.Name);
					
					HigMessageDialog md = new HigMessageDialog (main_window, DialogFlags.DestroyWithParent, 
										    Gtk.MessageType.Error, ButtonsType.Ok, 
										    msg,
										    desc);
					md.Run ();
					md.Destroy ();
				}
			
			}
		}

		dialog.Destroy ();
	}

	void HandleViewSmall (object sender, EventArgs args)
	{
		icon_view.ThumbnailWidth = 64;	
	}

	void HandleViewMedium (object sender, EventArgs args)
	{
		icon_view.ThumbnailWidth = 128;	
	}

	void HandleViewLarge (object sender, EventArgs args)
	{
		icon_view.ThumbnailWidth = 256;	
	}

	void HandleDisplayToolbar (object sender, EventArgs args)
	{
		if (display_toolbar.Active)
			toolbar.Show ();
		else
			toolbar.Hide ();
	}

	void HandleDisplayTags (object sender, EventArgs args)
	{
		icon_view.DisplayTags = !icon_view.DisplayTags;
	}
	
	void HandleDisplayDates (object sender, EventArgs args)
	{
		// Peg the icon_view's value to the MenuItem's active state,
		// as icon_view.DisplayDates's get won't always be equal to it's true value
		// because of logic to hide dates when zoomed way out.
		icon_view.DisplayDates = display_dates_menu_item.Active;
	}

	void HandleDisplayGroupSelector (object sender, EventArgs args)
	{
		if (group_selector.Visible)
			group_selector.Hide ();
		else
			group_selector.Show ();
	}

	void HandleDisplayInfoSidebar (object sender, EventArgs args)
	{
		if (info_vpaned.Visible)
			info_vpaned.Hide ();
		else
			info_vpaned.Show ();
	}

	public Gtk.Window Window {
		get {
			return main_window;
		}
	}
	
	void HandleViewSlideShow (object sender, EventArgs args)
	{
		main_window.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
		slide_delay.Start ();
	}

	private bool SlideShow ()
	{
		int [] ids = SelectedIds ();
		Photo [] photos = null;
		if (ids.Length < 2) {
			int i = 0;
			if (ids.Length > 0)
				i = ids [0];

			// FIXME this should be an  IBrowsableCollection.
			photos = new Photo [query.Photos.Length];
			Array.Copy (query.Photos, i, photos, 0, query.Photos.Length - i);
			Array.Copy (query.Photos, 0, photos, query.Photos.Length - i, i);
			System.Console.WriteLine (photos.Length);
		} else {
			photos = SelectedPhotos ();
		}

		if (photos.Length == 0) {
			Console.WriteLine ("No photos available -- no slideshow");
			main_window.GdkWindow.Cursor = null;
			return false;
		}

		FSpot.FullSlide full = new FSpot.FullSlide (main_window, photos);
		full.Play ();
		main_window.GdkWindow.Cursor = null;
		return false;
	}


	void HandleToggleViewBrowse (object sender, EventArgs args)
	{
	        ToggleButton toggle = sender as ToggleButton;
		if (toggle != null) {
			if (toggle.Active)
				SetViewMode (ModeType.IconView);
		} else
			SetViewMode (ModeType.IconView);
	}

	void HandleToggleViewPhoto (object sender, EventArgs args)
	{
	        ToggleButton toggle = sender as ToggleButton;
		
		if (toggle != null) {
			if (toggle.Active)
				SetViewMode (ModeType.PhotoView);
		} else
			SetViewMode (ModeType.IconView);
	}

	void HandleViewBrowse (object sender, EventArgs args)
	{
		SetViewMode (ModeType.IconView);
	}

	void HandleViewPhoto (object sender, EventArgs args)
	{
		SetViewMode (ModeType.PhotoView);
	}

	void HandleViewFullscreen (object sender, EventArgs args)
	{
		int active = Math.Max (ActiveIndex (), 0);
		if (fsview == null) {
			fsview = new FSpot.FullScreenView (query);
			fsview.Destroyed += HandleFullScreenViewDestroy;
		}
		// FIXME this needs to be another mode like PhotoView and IconView mode.
		fsview.View.Item.Index = active;

		fsview.Show ();
	}

	void HandleFullScreenViewDestroy (object sender, EventArgs args)
	{
		JumpTo (fsview.View.Item.Index);
		fsview = null;
	}
	
	void HandleZoomScaleValueChanged (object sender, System.EventArgs args)
	{
		switch (view_mode) {
		case ModeType.PhotoView:
			if (System.Math.Abs (photo_view.Zoom - zoom_scale.Value) > System.Double.Epsilon) 
				photo_view.Zoom = System.Math.Max (0.1, zoom_scale.Value);
			break;
		case ModeType.IconView:
			icon_view.ThumbnailWidth = (int)(System.Math.Max (15, zoom_scale.Value * 256));
			break;
		}
	}

	void HandleZoomChanged (object sender, System.EventArgs args)
	{
		switch (view_mode) {
		case ModeType.PhotoView:
			if (photo_view.Zoom != zoom_scale.Value) 
				zoom_scale.Value = photo_view.Zoom;
			break;
		case ModeType.IconView:
			break;
		}
	}

	void HandleZoomOut (object sender, EventArgs args)
	{
		switch (view_mode) {
		case ModeType.PhotoView:
			double old_zoom = photo_view.Zoom;

			old_zoom /= FSpot.PhotoImageView.ZoomMultipler;

			int offset_x, offset_y, scaled_width, scaled_height;
			photo_view.View.GetOffsets (out offset_x, out offset_y, out scaled_width, out scaled_height);

			if (scaled_width <= 256 && old_zoom < 1.0)
				SetViewMode (ModeType.IconView);
			else
				photo_view.Zoom = old_zoom;
			
			break;
		case ModeType.IconView:
			int width = icon_view.ThumbnailWidth;
			
			width /= 2;
			width = Math.Max (width, 64);
			width = Math.Min (width, 256);
			icon_view.ThumbnailWidth = width;

			break;
		}
	}

	void HandleZoomIn (object sender, EventArgs args)
	{
		switch (view_mode) {
		case ModeType.PhotoView:
			double old_zoom = photo_view.Zoom;
			try {
				photo_view.Zoom *= FSpot.PhotoImageView.ZoomMultipler;
			} catch {
				photo_view.Zoom = old_zoom;
			}
			
			break;
		case ModeType.IconView:
			int width = icon_view.ThumbnailWidth;
			 
			width *= 2;
			width = Math.Max (width, 64);
			if (width >= 512) {
				photo_view.Zoom = 0.0;
				SetViewMode (ModeType.PhotoView);
			} else {
				icon_view.ThumbnailWidth = width;
			}			
			break;
		}
	}
	
	public void HandleDeleteCommand (object sender, EventArgs args)
	{
   		Photo[] photos = SelectedPhotos();
   		string header = Mono.Posix.Catalog.GetPluralString ("Delete the selected photo permanently?", 
								 "Delete the {0} selected photos permanently?", 
								 photos.Length);
		header = String.Format (header, photos.Length);
		string msg = Mono.Posix.Catalog.GetPluralString ("This deletes all versions of the selected photo from your drive.", 
								 "This deletes all versions of the selected photos from your drive.", 
								 photos.Length);
		string ok_caption = Mono.Posix.Catalog.GetPluralString ("_Delete photo", "_Delete photos", photos.Length);
		if (ResponseType.Ok == HigMessageDialog.RunHigConfirmation(main_window, DialogFlags.DestroyWithParent, MessageType.Warning, header, msg, ok_caption)) {                              
			
			foreach (Photo photo in photos) {
				foreach (uint id in photo.VersionIds) {
					Console.WriteLine (" path == {0}", photo.GetVersionPath (id)); 
					photo.DeleteVersion (id, true);
				}
			}
			db.Photos.Remove (photos);
			
			UpdateQuery ();
		}
	}

	public void HandleRemoveCommand (object sender, EventArgs args)
	{
   		Photo[] photos = SelectedPhotos();
   		string header = Mono.Posix.Catalog.GetPluralString ("Remove the selected photo from F-Spot?",
								 "Remove the {0} selected photos from F-Spot?", 
								 photos.Length);

		header = String.Format (header, photos.Length);
		string msg = Mono.Posix.Catalog.GetString("If you remove photos from the F-Spot catalog all tag information will be lost. The photos remain on your computer and can be imported into F-Spot again.");
		string ok_caption = Mono.Posix.Catalog.GetString("_Remove from Catalog");
		if (ResponseType.Ok == HigMessageDialog.RunHigConfirmation(main_window, DialogFlags.DestroyWithParent, 
									   MessageType.Warning, header, msg, ok_caption)) {                              
			db.Photos.Remove (photos);
			UpdateQuery ();
		}
	}

	void HandleSelectAllCommand (object sender, EventArgs args)
	{
		icon_view.SelectAllCells ();
	}

	void HandleSelectNoneCommand (object sender, EventArgs args)
	{
		icon_view.Selection.Clear ();
	}

	public void HandleTagSelectionKeyPress (object sender, Gtk.KeyPressEventArgs args)
	{
		if (args.Event.Key == Gdk.Key.Delete)
			HandleDeleteSelectedTagCommand (sender, (EventArgs) args);
	}

	public void HandleDeleteSelectedTagCommand (object sender, EventArgs args)
	{
		Tag [] tags = this.tag_selection_widget.TagHighlight ();

		System.Array.Sort (tags, new TagRemoveComparer ());

		string header;
		if (tags.Length == 1)
			header = String.Format (Mono.Posix.Catalog.GetString ("Delete tag \"{0}\"?"), tags [0].Name);
		else
			header = String.Format (Mono.Posix.Catalog.GetString ("Delete the {0} selected tags?"), tags.Length);
		
		header = String.Format (header, tags.Length);
		string msg = Mono.Posix.Catalog.GetString("If you delete a tag, all associations with photos are lost.");
		string ok_caption = Mono.Posix.Catalog.GetPluralString ("_Delete tag", "_Delete tags", tags.Length);
		
		if (ResponseType.Ok == HigMessageDialog.RunHigConfirmation(main_window, 
									   DialogFlags.DestroyWithParent, 
									   MessageType.Warning, 
									   header, 
									   msg, 
									   ok_caption)) {                              
			try { 				
				db.Photos.Remove (tags);
			} catch (InvalidTagOperationException e) {
				System.Console.WriteLine ("this is something or another");

				// A Category is not empty. Can not delete it.
				string error_msg = Mono.Posix.Catalog.GetString ("Category is not empty");
				string error_desc = String.Format (Mono.Posix.Catalog.GetString ("Can not delete categories that have tags." + 
												 "Please delete tags under \"{0}\" first"),
								   e.Tag.Name);
				
				HigMessageDialog md = new HigMessageDialog (main_window, DialogFlags.DestroyWithParent, 
									    Gtk.MessageType.Error, ButtonsType.Ok, 
									    error_msg,
									    error_desc);
				md.Run ();
				md.Destroy ();
			}
		}
		icon_view.QueueDraw ();
	}

	void HandleUpdateThumbnailCommand (object sende, EventArgs args)
	{
		ThumbnailCommand command = new ThumbnailCommand (main_window);

		int [] selected_ids = SelectedIds ();
		if (command.Execute (SelectedPhotos (selected_ids))) {
			foreach (int num in selected_ids)
				query.MarkChanged (num);
		}
	}

	public void HandleRotate90Command (object sender, EventArgs args)
	{
		RotateSelectedPictures (RotateCommand.Direction.Clockwise);
	}

	public void HandleRotate270Command (object sender, EventArgs args)
	{
		RotateSelectedPictures (RotateCommand.Direction.Counterclockwise);
	}

	public void HandleCopyLocation (object sender, EventArgs args)
	{
		/*
		 * FIXME this should really set uri atoms as well as string atoms
		 */
		Clipboard primary = Clipboard.Get (Atom.Intern ("PRIMARY", false));
		Clipboard clipboard = Clipboard.Get (Atom.Intern ("CLIPBOARD", false));

		StringBuilder paths = new StringBuilder ();
		
		int i = 0;
		foreach (Photo p in SelectedPhotos ()) {
			if (i++ > 0)
				paths.Append (" ");

			paths.Append (System.IO.Path.GetFullPath (p.DefaultVersionPath));
		}
		
		String data = paths.ToString ();
		primary.Text = data;
		clipboard.Text = data;
	}

	void HandleSetAsBackgroundCommand (object sender, EventArgs args)
	{
		Photo current = CurrentPhoto;
		GConf.Client client = new GConf.Client ();
		
		if (current == null)
			return;

		client.Set ("/desktop/gnome/background/color_shading_type", "solid");
		client.Set ("/desktop/gnome/background/primary_color", "#000000");
		client.Set ("/desktop/gnome/background/picture_options", "scaled");
		client.Set ("/desktop/gnome/background/picture_opacity", 100);
		client.Set ("/desktop/gnome/background/picture_filename", current.DefaultVersionPath);
		client.Set ("/desktop/gnome/background/draw_background", true);
	}

	void HandleSetDateRange (object sender, EventArgs args) {
		DateCommands.Set set_command = new DateCommands.Set (query, main_window);
		set_command.Execute ();
	}

	void HandleClearDateRange (object sender, EventArgs args) {
		query.Range = null;
	}
	
	void OnPreferencesChanged (object sender, GConf.NotifyEventArgs args)
	{
		LoadPreference (args.Key);
	}

	void LoadPreference (String key)
	{
		object val = Preferences.Get (key);

		if (val == null)
			return;
		
		//System.Console.WriteLine("Setting {0} to {1}", key, val);

		switch (key) {
		case Preferences.MAIN_WINDOW_MAXIMIZED:
			if ((bool) val)
				main_window.Maximize ();
			else
				main_window.Unmaximize ();
			break;

		case Preferences.MAIN_WINDOW_X:
		case Preferences.MAIN_WINDOW_Y:
			main_window.Move((int) Preferences.Get(Preferences.MAIN_WINDOW_X),
					(int) Preferences.Get(Preferences.MAIN_WINDOW_Y));
			break;
		
		case Preferences.MAIN_WINDOW_WIDTH:
		case Preferences.MAIN_WINDOW_HEIGHT:
			main_window.SetDefaultSize((int) Preferences.Get(Preferences.MAIN_WINDOW_WIDTH),
					(int) Preferences.Get(Preferences.MAIN_WINDOW_HEIGHT));

			main_window.ReshowWithInitialSize();
			break;
		
		case Preferences.SHOW_TOOLBAR:
			if (display_toolbar.Active != (bool) val)
				display_toolbar.Active = (bool) val;
			break;
		
		case Preferences.SHOW_SIDEBAR:
			if (display_sidebar.Active != (bool) val)
				display_sidebar.Active = (bool) val;
			break;
		
		case Preferences.SHOW_TIMELINE:
			if (display_timeline.Active != (bool) val)
				display_timeline.Active = (bool) val;
			break;
		
		case Preferences.SHOW_TAGS:
			if (display_tags_menu_item.Active != (bool) val)
				display_tags_menu_item.Active = (bool) val;
			break;
		
		case Preferences.SHOW_DATES:
			if (display_dates_menu_item.Active != (bool) val)
				display_dates_menu_item.Active = (bool) val;
				//display_dates_menu_item.Toggle ();
			break;
		
		case Preferences.SIDEBAR_POSITION:
			if (main_hpaned.Position != (int) val)
				main_hpaned.Position = (int) val;
			break;
		
		case Preferences.THUMBNAIL_WIDTH:
			if (icon_view.ThumbnailWidth != (int) val)
				icon_view.ThumbnailWidth = (int) val;
			break;
		
		case Preferences.ICON_VIEW_POSITION:
			if (icon_view.TopLeftVisibleCell () != (int) val) {
				icon_view.FocusCell = (int) val;
				photo_view.Item.Index = (int) val;
				icon_view.ScrollTo ((int) val, false);
			}
			break;
		}
	}

	// Version Id updates.

	void UpdateForVersionIdChange (uint version_id)
	{
		CurrentPhoto.DefaultVersionId = version_id;
		int active = ActiveIndex ();
		
		query.Commit (active);
	}

	void HandleVersionIdChanged (PhotoVersionMenu menu)
	{
		UpdateForVersionIdChange (menu.VersionId);
	}

	void HandleInfoBoxVersionIdChange (InfoBox box, uint version_id)
	{
		UpdateForVersionIdChange (version_id);
	}


	// Queries.

	void UpdateQuery ()
	{
		main_window.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
		main_window.GdkWindow.Display.Sync ();
		query.Tags = tag_selection_widget.TagSelection;
		main_window.GdkWindow.Cursor = null;
	}

	void OnTagSelectionChanged (object obj)
	{
		UpdateQuery ();
		SetViewMode (ModeType.IconView);
	}
	
	void HandleTagSelectionChanged (object obj, EventArgs args)
	{
		UpdateMenus ();
	}

	void HandleQueryItemChanged (FSpot.IBrowsableCollection browsable, int item)
	{
		if (photo_view.Item.Index == item) {
			UpdateMenus ();
			info_box.Update ();
		}
	}
	//
	// Handle Main Menu 

	void UpdateMenus ()
	{
		bool tag_sensitive = tag_selection_widget.Selection.CountSelectedRows () > 0;
		bool active_selection = PhotoSelectionActive ();
		bool single_active = CurrentPhoto != null;
		
		if (!single_active) {
			version_menu_item.Sensitive = false;
			version_menu_item.Submenu = new Menu ();

			create_version_menu_item.Sensitive = false;
			delete_version_menu_item.Sensitive = false;
			rename_version_menu_item.Sensitive = false;

		} else {
			version_menu_item.Sensitive = true;
			create_version_menu_item.Sensitive = true;
			
			if (CurrentPhoto.DefaultVersionId == Photo.OriginalVersionId) {
				delete_version_menu_item.Sensitive = false;
				rename_version_menu_item.Sensitive = false;
			} else {
				delete_version_menu_item.Sensitive = true;
				rename_version_menu_item.Sensitive = true;
			}

			versions_submenu = new PhotoVersionMenu (CurrentPhoto);
			versions_submenu.VersionIdChanged += new PhotoVersionMenu.VersionIdChangedHandler (HandleVersionIdChanged);
			version_menu_item.Submenu = versions_submenu;
		}

		set_as_background.Sensitive = single_active;
		adjust_color.Sensitive = single_active;

		attach_tag.Sensitive = active_selection;
		remove_tag.Sensitive = active_selection;

		rotate_left.Sensitive = active_selection;
		rotate_right.Sensitive = active_selection;
		update_thumbnail.Sensitive = active_selection;
		delete_from_drive.Sensitive = active_selection;
		
		send_mail.Sensitive = active_selection;
		print.Sensitive = active_selection;
		export.Sensitive = active_selection;
		select_none.Sensitive = active_selection;
		copy_location.Sensitive = active_selection;
		exif_data.Sensitive = active_selection;
		sharpen.Sensitive = active_selection;
		remove_from_catalog.Sensitive = active_selection;

		delete_selected_tag.Sensitive = tag_sensitive;
		edit_selected_tag.Sensitive = tag_sensitive;

		attach_tag_to_selection.Sensitive = tag_sensitive && active_selection;
		remove_tag_from_selection.Sensitive = tag_sensitive && active_selection;
	}

	// Tag typing

	private ArrayList selected_photos_tagnames;

	private void UpdateTagEntryFromSelection ()
	{
		Hashtable taghash = new Hashtable ();

		Photo [] sel = SelectedPhotos ();
		for (int i = 0; i < sel.Length; i++) {
			foreach (Tag tag in sel [i].Tags) {
				int count = 1;

				if (taghash.Contains (tag))
					count = ((int) taghash [tag]) + 1;

				if (count <= i)
					taghash.Remove (tag);
				else 
					taghash [tag] = count;
			}
			
			if (taghash.Count == 0)
				break;
		}

		selected_photos_tagnames = new ArrayList ();
		foreach (Tag tag in taghash.Keys)
			if ((int) (taghash [tag]) == sel.Length) {
				System.Console.WriteLine (tag.Name);
				selected_photos_tagnames.Add (tag.Name);
			}

		selected_photos_tagnames.Sort ();

		StringBuilder sb = new StringBuilder ();
		foreach (string tagname in selected_photos_tagnames) {
			if (sb.Length > 0)
				sb.Append (", ");

			sb.Append (tagname);
		}

		tag_entry.Text = sb.ToString ();
		ClearTagCompletions ();
	}

	public void HandlePossibleTagTyping (object sender, Gtk.KeyPressEventArgs args)
	{
		if (tagbar.Visible && tag_entry.HasFocus)
			return;

#if !ALLOW_TAG_TYPING_WITHOUT_HOTKEY
		if (args.Event.Key != Gdk.Key.t)
			return;
#endif

#if ALLOW_TAG_TYPING_WITHOUT_HOTKEY
		char c = System.Convert.ToChar (Gdk.Keyval.ToUnicode ((uint) args.Event.Key));
		if (! System.Char.IsLetter (c))
			return;
#endif
		
		if (tag_entry.Text.Length > 0)
			tag_entry.Text += ", ";

#if ALLOW_TAG_TYPING_WITHOUT_HOTKEY
		tag_entry.Text += c;
#endif

		tagbar.Show ();
		tag_entry.GrabFocus ();
		tag_entry.SelectRegion (-1, -1);
	}

	// "Activate" means the user pressed the enter key
	public void HandleTagEntryActivate (object sender, EventArgs args)
	{
		string [] tagnames = GetTypedTagNames ();
		int [] selected_photos = SelectedIds ();

		if (selected_photos == null || tagnames == null)
			return;

               int sel_start, sel_end;
               if (tag_entry.GetSelectionBounds (out sel_start, out sel_end) && tag_completion_index != -1) {
                       tag_entry.InsertText (", ", ref sel_end);
                       tag_entry.SelectRegion (-1, -1);
                       tag_entry.Position = sel_end + 2;
                       ClearTagCompletions ();
                       return;
               }


	       // Add any new tags to the selected photos
	       Category default_category = null;
	       Tag [] selection = tag_selection_widget.TagHighlight ();
	       if (selection.Length > 0) {
		       if (selection [0] is Category)
			       default_category = (Category) selection [0];
		       else
			       default_category = selection [0].Category;
	       }

	       foreach (string tagname in tagnames) {
		       if (tagname.Length == 0)
			       continue;

		       Tag t = db.Tags.GetTagByName (tagname);
		       if (t == null) {
			       t = db.Tags.CreateTag (default_category, tagname);
			       db.Tags.Commit (t);
		       }

		       Tag [] tags = new Tag [1];
		       tags [0] = t;

		       foreach (int num in selected_photos)
			       AddTagExtended (num, tags);
	       }

	       // Remove any removed tags from the selected photos
	       foreach (string tagname in selected_photos_tagnames) {
		       if (! IsTagInList (tagnames, tagname)) {
				
			       Tag tag = db.Tags.GetTagByName (tagname);

			       foreach (int num in selected_photos) {
				       query.Photos [num].RemoveTag (tag);
				       query.Commit (num);
			       }
		       }
	       }

	       UpdateTagEntryFromSelection ();
	       if (view_mode == ModeType.IconView) {
		       icon_view.QueueDraw ();
		       icon_view.GrabFocus ();
	       } else {
		       photo_view.QueueDraw ();
		       photo_view.View.GrabFocus ();
	       }
	}

	private void HideTagbar ()
	{
		if (! tagbar.Visible)
			return;
		
		// Cancel any pending edits...
		UpdateTagEntryFromSelection ();

		tagbar.Hide ();

		if (view_mode == ModeType.IconView)
			icon_view.GrabFocus ();
		else
			photo_view.View.GrabFocus ();

		ClearTagCompletions ();
	}

	public void HandleTagBarCloseButtonPressed (object sender, EventArgs args)
	{
		HideTagbar ();
	}

	public void HandleTagEntryKeyPressEvent (object sender, Gtk.KeyPressEventArgs args)
	{
		args.RetVal = false;

		if (args.Event.Key == Gdk.Key.Escape) { 
			HideTagbar ();
			args.RetVal = true;
		} else if (args.Event.Key == Gdk.Key.Tab) {
			DoTagCompletion ();
			args.RetVal = true;
		} else
			ClearTagCompletions ();
	}

	bool tag_ignore_changes = false;
	public void HandleTagEntryChanged (object sender, EventArgs args)
	{
		if (tag_ignore_changes)
			return;

		ClearTagCompletions ();
	}

	int tag_completion_index = -1;
	Tag [] tag_completions;
	string tag_completion_typed_so_far;
	int tag_completion_typed_position;
	private void DoTagCompletion ()
	{
		string completion;
		
		if (tag_completion_index != -1) {
			tag_completion_index = (tag_completion_index + 1) % tag_completions.Length;
		} else {

			tag_completion_typed_position = tag_entry.Position;
		    
			string right_of_cursor = tag_entry.Text.Substring (tag_completion_typed_position);
			if (right_of_cursor.Length > 1)
				return;

			int last_comma = tag_entry.Text.LastIndexOf (',');
			if (last_comma > tag_completion_typed_position)
				return;

			tag_completion_typed_so_far = tag_entry.Text.Substring (last_comma + 1).TrimStart (new char [] {' '});
			if (tag_completion_typed_so_far == null || tag_completion_typed_so_far.Length == 0)
				return;

			tag_completions = db.Tags.GetTagsByNameStart (tag_completion_typed_so_far);
			if (tag_completions == null)
				return;

			tag_completion_index = 0;
		}

		tag_ignore_changes = true;
		completion = tag_completions [tag_completion_index].Name.Substring (tag_completion_typed_so_far.Length);
		tag_entry.Text = tag_entry.Text.Substring (0, tag_completion_typed_position) + completion;
		tag_ignore_changes = false;

		tag_entry.Position = tag_entry.Text.Length;
		tag_entry.SelectRegion (tag_completion_typed_position, tag_entry.Text.Length);
	}

	private void ClearTagCompletions ()
	{
		tag_completion_index = -1;
		tag_completions = null;
	}


	public void HandleTagEntryFocusOutEvent (object sender, EventArgs args)
	{
		UpdateTagEntryFromSelection ();
		//		HideTagbar ();
	}

	private string [] GetTypedTagNames ()
	{
		string [] tagnames = tag_entry.Text.Split (new char [] {','});

		ArrayList list = new ArrayList ();
		for (int i = 0; i < tagnames.Length; i ++) {
			string s = tagnames [i].Trim ();

			if (s.Length > 0)
				list.Add (s);
		}

		return (string []) (list.ToArray (typeof (string)));
	}

	private bool IsTagInList (string [] tags, string tag)
	{
		foreach (string t in tags)
			if (t == tag)
				return true;
		return false;
	}
}
