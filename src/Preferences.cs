using System.Net;
using System;
using System.Collections.Generic;
using Mono.Unix;
using FSpot.Platform;

namespace FSpot
{
	public class Preferences
	{
		public const string APP_FSPOT = "/apps/f-spot/";
		public const string APP_FSPOT_EXPORT = APP_FSPOT + "export/";
		public const string APP_FSPOT_EXPORT_TOKENS = APP_FSPOT_EXPORT + "tokens/";

		public const string GTK_RC = APP_FSPOT + "ui/gtkrc";

		public const string MAIN_WINDOW_MAXIMIZED = APP_FSPOT + "ui/maximized";
		public const string MAIN_WINDOW_X = APP_FSPOT + "ui/main_window_x";
		public const string MAIN_WINDOW_Y = APP_FSPOT + "ui/main_window_y";
		public const string MAIN_WINDOW_WIDTH = APP_FSPOT + "ui/main_window_width";
		public const string MAIN_WINDOW_HEIGHT = APP_FSPOT + "ui/main_window_height";

		public const string IMPORT_WINDOW_WIDTH = APP_FSPOT + "ui/import_window_width";
		public const string IMPORT_WINDOW_HEIGHT = APP_FSPOT + "ui/import_window_height";
		public const string IMPORT_WINDOW_PANE_POSITION = APP_FSPOT + "ui/import_window_pane_position";

		public const string IMPORT_COPY_FILES = "/apps/f-spot/import/copy_files";
		public const string IMPORT_INCLUDE_SUBFOLDERS = "/apps/f-spot/import/include_subfolders";
		public const string IMPORT_CHECK_DUPLICATES = "/apps/f-spot/import/check_duplicates";
		
		public const string VIEWER_WIDTH = APP_FSPOT + "ui/viewer_width";
		public const string VIEWER_HEIGHT = APP_FSPOT + "ui/viewer_height";
		public const string VIEWER_MAXIMIZED = APP_FSPOT + "ui/viewer_maximized";
		public const string VIEWER_SHOW_TOOLBAR = APP_FSPOT + "ui/viewer_show_toolbar";
		public const string VIEWER_SHOW_FILENAMES = APP_FSPOT + "ui/viewer_show_filenames";
		public const string VIEWER_INTERPOLATION = APP_FSPOT + "viewer/interpolation";
		public const string VIEWER_TRANS_COLOR = APP_FSPOT + "viewer/trans_color";
		public const string VIEWER_TRANSPARENCY = APP_FSPOT + "viewer/transparency";
		public const string CUSTOM_CROP_RATIOS = APP_FSPOT + "viewer/custom_crop_ratios";
		
		public const string COLOR_MANAGEMENT_DISPLAY_PROFILE = APP_FSPOT + "ui/color_management_display_profile";
		public const string COLOR_MANAGEMENT_OUTPUT_PROFILE = APP_FSPOT + "ui/color_management_output_profile";
		
		public const string SHOW_TOOLBAR = APP_FSPOT + "ui/show_toolbar";
		public const string SHOW_SIDEBAR = APP_FSPOT + "ui/show_sidebar";
		public const string SHOW_TIMELINE = APP_FSPOT + "ui/show_timeline";
		public const string SHOW_FILMSTRIP = APP_FSPOT + "ui/show_filmstrip";
		public const string FILMSTRIP_ORIENTATION = APP_FSPOT + "ui/filmstrip_orientation";
		public const string SHOW_TAGS = APP_FSPOT + "ui/show_tags";
		public const string SHOW_DATES = APP_FSPOT + "ui/show_dates";
		public const string EXPANDED_TAGS = APP_FSPOT + "ui/expanded_tags";
		public const string SHOW_RATINGS = APP_FSPOT + "ui/show_ratings";
		public const string TAG_ICON_SIZE = APP_FSPOT + "ui/tag_icon_size";
		public const string TAG_ICON_AUTOMATIC = APP_FSPOT + "ui/tag_icon_automatic";
		
		public const string GLASS_POSITION = APP_FSPOT + "ui/glass_position";
		public const string GROUP_ADAPTOR_ORDER_ASC = APP_FSPOT + "ui/group_adaptor_sort_asc";
		
		public const string SIDEBAR_POSITION = APP_FSPOT + "ui/sidebar_size";
		public const string ZOOM = APP_FSPOT + "ui/zoom";

		public const string EXPORT_EMAIL_SIZE = APP_FSPOT + "export/email/size";
		public const string EXPORT_EMAIL_ROTATE = APP_FSPOT + "export/email/auto_rotate";
		public const string EXPORT_EMAIL_DELETE_TIMEOUT_SEC = APP_FSPOT + "export/email/delete_timeout_seconds";

		public const string IMPORT_GUI_ROLL_HISTORY = APP_FSPOT + "import/gui_roll_history";

		public const string SCREENSAVER_TAG = APP_FSPOT + "screensaver/tag_id";
		public const string SCREENSAVER_DELAY = APP_FSPOT + "screensaver/delay";

		public const string STORAGE_PATH = APP_FSPOT + "import/storage_path";

		public const string METADATA_EMBED_IN_IMAGE = APP_FSPOT + "metadata/embed_in_image";

		public const string EDIT_REDEYE_THRESHOLD = APP_FSPOT + "edit/redeye_threshold";
		public const string EDIT_CREATE_XCF_VERSION = APP_FSPOT + "edit/create_xcf";

		public const string GNOME_MAILTO = "/desktop/gnome/url-handlers/mailto/";
		public const string GNOME_MAILTO_COMMAND = GNOME_MAILTO + "command";
		public const string GNOME_MAILTO_ENABLED = GNOME_MAILTO + "enabled";

		public const string GSD_THUMBS_MAX_AGE = "/desktop/gnome/thumbnail_cache/maximum_age";
		public const string GSD_THUMBS_MAX_SIZE = "/desktop/gnome/thumbnail_cache/maximum_size";


		private static PreferenceBackend backend;
		private static EventHandler<NotifyEventArgs> changed_handler;
		private static PreferenceBackend Backend {
			get {
				if (backend == null) {
					backend = new PreferenceBackend ();
					changed_handler = new EventHandler<NotifyEventArgs> (OnSettingChanged);
					backend.AddNotify (APP_FSPOT, changed_handler);
					backend.AddNotify (GNOME_MAILTO, changed_handler);
				}
				return backend;
			}
		}

		private static Dictionary<string, object> cache = new Dictionary<string, object>();

		static object GetDefault (string key)
		{
			switch (key) {
			case MAIN_WINDOW_X:
			case MAIN_WINDOW_Y:
			case MAIN_WINDOW_HEIGHT:
			case MAIN_WINDOW_WIDTH:
			case IMPORT_WINDOW_HEIGHT:
			case IMPORT_WINDOW_WIDTH:
			case IMPORT_WINDOW_PANE_POSITION:
			case FILMSTRIP_ORIENTATION:
				return 0;
					
			case METADATA_EMBED_IN_IMAGE:
			case MAIN_WINDOW_MAXIMIZED:
			case GROUP_ADAPTOR_ORDER_ASC:
				return false;

			case GLASS_POSITION:
				return null;

			case SHOW_TOOLBAR:
			case SHOW_SIDEBAR:
			case SHOW_TIMELINE:
			case SHOW_FILMSTRIP:
			case SHOW_TAGS:
			case SHOW_DATES:
			case SHOW_RATINGS:
			case VIEWER_SHOW_FILENAMES:
				return true;
			
			case TAG_ICON_SIZE:
				return (int) Tag.IconSize.Medium;

			case TAG_ICON_AUTOMATIC:
				return true;
		
			case SIDEBAR_POSITION:
				return 130;
			case ZOOM:
				return null;

			case IMPORT_GUI_ROLL_HISTORY:
				return 10;

			case SCREENSAVER_TAG:
				return 1;
			case SCREENSAVER_DELAY:
				return 4.0;
			case STORAGE_PATH:
				return System.IO.Path.Combine (FSpot.Global.HomeDirectory, Catalog.GetString("Photos"));
			case EXPORT_EMAIL_SIZE:
				return 3;	// medium size 640px
			case EXPORT_EMAIL_ROTATE:
			case VIEWER_INTERPOLATION:
				return true;
			case EXPORT_EMAIL_DELETE_TIMEOUT_SEC:
				return 30;	// delete temporary email pictures after 30 seconds
			case VIEWER_TRANSPARENCY:
				return "NONE";
			case VIEWER_TRANS_COLOR:
				return "#000000";
			case EDIT_REDEYE_THRESHOLD:
				return -15;

			case GTK_RC:
			case COLOR_MANAGEMENT_DISPLAY_PROFILE:
			case COLOR_MANAGEMENT_OUTPUT_PROFILE:
				return String.Empty;
			case IMPORT_CHECK_DUPLICATES:
			case IMPORT_COPY_FILES:
			case IMPORT_INCLUDE_SUBFOLDERS:
				return true;
			default:
				return null;
			}
		}
		
		//return true if the key exists in the backend
		public static bool TryGet<T> (string key, out T value)
		{
			lock (cache) {
				value = default (T);
				object o;
				if (cache.TryGetValue (key, out o)) {
					value = (T)o;
					return true;
				}

				try {
					value = (T) Backend.Get (key);
				} catch { //catching NoSuchKeyException
					return false;
				}
				
				cache.Add (key, value);
				return true;
			}
		}

		public static T Get<T> (string key)
		{
			T val;
			if (TryGet<T> (key, out val))
				return val;
			try {
				return (T) GetDefault (key);
			} catch { //catching InvalidCastException
				return default (T);
			}
		}

		public static void Set (string key, object value)
		{
			lock (cache) {
				try {
					cache [key] = value;				
					Backend.Set (key, value);
				} catch (Exception e){
					Console.WriteLine (e);
					Console.WriteLine ("Unable to set this :"+key);
				}
			}
		}

		public static event EventHandler<NotifyEventArgs> SettingChanged;

		static void OnSettingChanged (object sender, NotifyEventArgs args)
		{
			lock (cache) {
				if (cache.ContainsKey (args.Key)) {
					cache [args.Key] = args.Value;				
				}
			}

			if (SettingChanged != null)
				SettingChanged (sender, args);
		}

	}
}
