# Initializers
MONO_BASE_PATH = 
MONO_ADDINS_PATH =

# Install Paths
DEFAULT_INSTALL_DIR = $(pkglibdir)
BACKENDS_INSTALL_DIR = $(DEFAULT_INSTALL_DIR)/Backends
EXTENSIONS_INSTALL_DIR = $(DEFAULT_INSTALL_DIR)/Extensions


## Directories
DIR_DOCS = $(top_builddir)/docs
DIR_EXTENSIONS = $(top_builddir)/extensions
DIR_ICONS = $(top_builddir)/icons
DIR_LIBFSPOT = $(top_builddir)/lib/libfspot
DIR_SEMWEB = $(top_builddir)/lib/semweb
DIR_SRC = $(top_builddir)/src
DIR_GIOSHARP = $(top_builddir)/lib/gio-sharp/gio
DIR_GTKSHARPBEANS = $(top_builddir)/lib/gtk-sharp-beans
DIR_UNIQUESHARP = $(top_builddir)/lib/unique-sharp/unique
DIR_TAGLIB = $(top_builddir)/lib/TagLib
DIR_BIN = $(top_builddir)/bin


# External libraries to link against, generated from configure
LINK_SYSTEM = -r:System
LINK_SYSTEMDATA = -r:System.Data
LINK_SYSTEM_WEB = -r:System.Web
LINK_MONO_POSIX = -r:Mono.Posix
LINK_MONO_CAIRO = -r:Mono.Cairo
LINK_MONO_SIMD = -r:Mono.Simd
LINK_MONODATA = -r:Mono.Data
LINK_MONO_DATA_SQLITECLIENT = -r:Mono.Data.SqliteClient
LINK_ICSHARP_ZIP_LIB = -r:ICSharpCode.SharpZipLib

LINK_KEYRING = $(KEYRINGSHARP_LIBS)
LINK_GLIB = $(GLIBSHARP_LIBS)
LINK_GTK = $(GTKSHARP_LIBS)
LINK_GNOME = $(GNOME_SHARP_LIBS)
LINK_GCONF = $(GCONF_SHARP_LIBS)
LINK_GLADE = -pkg:glade-sharp-2.0
LINK_FLICKRNET = -pkg:flickrnet
LINK_DBUS = $(NDESK_DBUS_LIBS) $(NDESK_DBUS_GLIB_LIBS)
LINK_DBUS_NO_GLIB = $(NDESK_DBUS_LIBS)
LINK_MONO_ADDINS_DEPS = $(MONO_ADDINS_LIBS)
LINK_MONO_ADDINS_SETUP_DEPS = $(MONO_ADDINS_SETUP_LIBS)
LINK_MONO_ADDINS_GUI_DEPS = $(MONO_ADDINS_GUI_LIBS)

# GIO
REF_GIO =
LINK_GIO = -r:$(DIR_GIOSHARP)/gio-sharp.dll
LINK_GIO_DEPS = $(REF_GIO) $(LINK_GIO)

# Gtk Beans
REF_GTK_BEANS = $(LINK_GIO_DEPS)
LINK_GTK_BEANS = -r:$(DIR_GTKSHARPBEANS)/gtk-sharp-beans.dll
LINK_GTK_BEANS_DEPS = $(REF_GTK_BEANS) $(LINK_GTK_BEANS)

# Uniqe
REF_UNIQUE =
LINK_UNIQUE = -r:$(DIR_UNIQUESHARP)/unique-sharp.dll
LINK_UNIQUE_DEPS = $(REF_UNIQUE) $(LINK_UNIQUE)

# Hyena
REF_HYENA = $(LINK_SYSTEM) $(LINK_MONO_POSIX)
LINK_HYENA = -r:$(DIR_BIN)/Hyena.dll
LINK_HYENA_DEPS = $(REF_HYENA) $(LINK_HYENA)

# Semweb
REF_SEMWEB = $(LINK_SYSTEMDATA) $(LINK_HYENA_DEPS)
LINK_SEMWEB = -r:$(DIR_BIN)/SemWeb.dll
LINK_SEMWEB_DEPS = $(REF_SEMWEB) $(LINK_SEMWEB)

# TagLib
REF_TAGLIB =
LINK_TAGLIB = -r:$(DIR_BIN)/TagLib.dll
LINK_TAGLIB_DEPS = $(REF_TAGLIB) $(LINK_TAGLIB)

# Hyena.Data.Sqlite
REF_HYENA_DATA_SQLITE = $(LINK_SQLITE)
LINK_HYENA_DATA_SQLITE = -r:$(DIR_BIN)/Hyena.Data.Sqlite.dll
LINK_HYENA_DATA_SQLITE_DEPS = $(REF_HYENA_DATA_SQLITE) $(LINK_HYENA_DATA_SQLITE)

# Hyena.Gui
REF_HYENA_GUI = $(LINK_HYENA_DEPS)
LINK_HYENA_GUI = -r:$(DIR_BIN)/Hyena.Gui.dll
LINK_HYENA_GUI_DEPS = $(REF_HYENA_GUI) $(LINK_HYENA_GUI)

# FSpot.Cms
REF_FSPOT_CMS = $(LINK_GTK)
LINK_FSPOT_CMS = -r:$(DIR_BIN)/FSpot.Cms.dll
LINK_FSPOT_CMS_DEPS = $(REF_FSPOT_CMS) $(LINK_FSPOT_CMS)

# FSpot.Utils
REF_FSPOT_UTILS = $(LINK_HYENA_DEPS) $(LINK_GTK) $(LINK_GIO) $(LINK_MONO_CAIRO) $(LINK_TAGLIB)
LINK_FSPOT_UTILS = -r:$(DIR_BIN)/FSpot.Utils.dll
LINK_FSPOT_UTILS_DEPS = $(REF_FSPOT_UTILS) $(LINK_FSPOT_UTILS)

# FSpot.Core
REF_FSPOT_CORE = $(LINK_FSPOT_UTILS_DEPS) $(LINK_FSPOT_CMS_DEPS)
LINK_FSPOT_CORE = -r:$(DIR_BIN)/FSpot.Core.dll
LINK_FSPOT_CORE_DEPS = $(REF_FSPOT_CORE) $(LINK_FSPOT_CORE)

# FSpot.Query
REF_FSPOT_QUERY = $(LINK_FSPOT_CORE_DEPS)
LINK_FSPOT_QUERY = -r:$(DIR_BIN)/FSpot.Query.dll
LINK_FSPOT_QUERY_DEPS = $(REF_FSPOT_QUERY) $(LINK_FSPOT_QUERY)

# FSpot.JobScheduler
REF_FSPOT_JOB_SCHEDULER = $(LINK_HYENA_DEPS)
LINK_FSPOT_JOB_SCHEDULER = -r:$(DIR_BIN)/FSpot.JobScheduler.dll
LINK_FSPOT_JOB_SCHEDULER_DEPS = $(REF_FSPOT_JOB_SCHEDULER) $(LINK_FSPOT_JOB_SCHEDULER)

# FSpot.Bling
REF_FSPOT_BLING = $(LINK_GTK_BEANS_DEPS) $(LINK_GLIB)
LINK_FSPOT_BLING = -r:$(DIR_BIN)/FSpot.Bling.dll
LINK_FSPOT_BLING_DEPS = $(REF_FSPOT_BLING) $(LINK_FSPOT_BLING)

# FSpot.Platform
REF_FSPOT_PLATFORM = $(LINK_GCONF) $(LINK_GTK) $(LINK_FSPOT_CORE_DEPS) $(LINK_DBUS)
LINK_FSPOT_PLATFORM = -r:$(DIR_BIN)/FSpot.Platform.dll
LINK_FSPOT_PLATFORM_DEPS = $(REF_FSPOT_PLATFORM) $(LINK_FSPOT_PLATFORM)

# FSpot.Widgets
REF_FSPOT_WIDGETS = $(LINK_FSPOT_CORE_DEPS) $(LINK_FSPOT_BLING_DEPS)
LINK_FSPOT_WIDGETS = -r:$(DIR_BIN)/FSpot.Widgets.dll
LINK_FSPOT_WIDGETS_DEPS = $(REF_FSPOT_WIDGETS) $(LINK_FSPOT_WIDGETS) $(LINK_HENA_GUI_DEPS)

# FSpot (executable)
REF_FSPOT = $(LINK_FSPOT_WIDGETS_DEPS) $(LINK_FSPOT_PLATFORM_DEPS) $(LINK_FSPOT_QUERY_DEPS) \
            $(LINK_GLIB) $(LINK_MONO_ADDINS_DEPS) $(LINK_UNIQUE_DEPS) $(LINK_MONO_ADDINS_SETUP_DEPS) \
            $(LINK_SEMWEB_DEPS) $(LINK_GLADE) $(LINK_MONODATA) $(LINK_MONO_DATA_SQLITECLIENT) \
            $(LINK_MONO_ADDINS_GUI_DEPS) $(LINK_FSPOT_JOB_SCHEDULER_DEPS) $(LINK_ICSHARP_ZIP_LIB) \
            $(LINK_GNOME) $(LINK_HYENA_GUI_DEPS) $(LINK_TAGLIB)
# FIXME: do not link executables
LINK_FSPOT = -r:$(DIR_BIN)/f-spot.exe
LINK_FSPOT_DEPS = $(REF_FSPOT) $(LINK_FSPOT)

# Extensions
REF_FSPOT_EXTENSION_BLACKOUTEDITOR = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_BWEDITOR = $(LINK_FSPOT_DEPS) $(LINK_MONO_SIMD)
REF_FSPOT_EXTENSION_FLIPEDITOR = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_PIXELATEEDITOR = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_RESIZEEDITOR = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_CDEXPORT = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_FACEBOOKEXPORT = $(LINK_FSPOT_DEPS) $(LINK_KEYRING)
REF_FSPOT_EXTENSION_FLICKREXPORT = $(LINK_FSPOT_DEPS) $(LINK_FLICKRNET)
REF_FSPOT_EXTENSION_FOLDEREXPORT = $(LINK_FSPOT_DEPS) $(LINK_SYSTEM_WEB)
REF_FSPOT_EXTENSION_GALLERYEXPORT = $(LINK_FSPOT_DEPS)

REF_MONO_GOOGLE = $(LINK_HYENA_DEPS)
LINK_MONO_GOOGLE = -r:$(DIR_BIN)/Mono.Google.dll
LINK_MONO_GOOGLE_DEPS = $(REF_MONO_GOOGLE) $(LINK_MONO_GOOGLE)
REF_FSPOT_EXTENSION_PICASAWEBEXPORT = $(LINK_FSPOT_DEPS) $(LINK_MONO_GOOGLE) $(LINK_KEYRING)

REF_SMUGMUGNET = $(LINK_FSPOT_DEPS)
LINK_SMUGMUGNET = -r:$(DIR_BIN)/SmugMugNet.dll
LINK_SMUGMUGNET_DEPS = $(REF_SMUGMUGNET) $(LINK_SMUGMUGNET)
REF_FSPOT_EXTENSION_SMUGMUGEXPORT = $(LINK_SMUGMUGNET_DEPS) $(LINK_KEYRING)

REF_MONO_TABBLO = $(LINK_HYENA_DEPS)
LINK_MONO_TABBLO = -r:$(DIR_BIN)/Mono.Tabblo.dll
LINK_MONO_TABBLO_DEPS = $(REF_MONO_TABBLO) $(LINK_MONO_TABBLO)
REF_FSPOT_EXTENSION_TABBLOEXPORT = $(LINK_FSPOT_DEPS) $(LINK_KEYRING) $(LINK_MONO_TABBLO_DEPS)

REF_FSPOT_EXTENSION_ZIPEXPORT = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_CHANGEPHOTOPATH = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_DEVELOPINUFRAW = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_HASHJOB = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_LIVEWEBGALLERY = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_MERGEDB = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_RAWPLUSJPEG = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_RETROACTIVEROLL = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_SCREENSAVERCONFIG = $(LINK_FSPOT_DEPS)
REF_FSPOT_EXTENSION_COVERTRANSITION = $(LINK_FSPOT_DEPS)




# Cute hack to replace a space with something
colon:= :
empty:=
space:= $(empty) $(empty)

# Build path to allow running uninstalled
RUN_PATH = $(subst $(space),$(colon), $(MONO_BASE_PATH))

