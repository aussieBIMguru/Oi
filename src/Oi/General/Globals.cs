using System.Collections;
using System.Globalization;
using System.Resources;
using Assembly = System.Reflection.Assembly;
using Autodesk.Revit.ApplicationServices;
using System.IO;

// The class belongs to the root namespace
namespace Oi
{
    /// <summary>
    /// Variables that persist beyond the running of commands.
    /// They can be accessed via this static class where they would otherwise be inaccessible.
    /// </summary>
    public static class Globals
    {
        #region Apps, Documents and Views

        /// <summary>
        /// The UIControlledApplication for the session.
        /// 
        /// Variable stored on App startup - not safe to call until Idling.
        /// </summary>
        public static UIControlledApplication UICTLAPP { get; set; }

        /// <summary>
        /// The UIApplication for the session.
        /// 
        /// Variable stored on App startup - not safe to call until Idling.
        /// </summary>
        public static UIApplication UIAPP { get; set; }

        /// <summary>
        /// The ControlledApplication for the session.
        /// </summary>
        public static ControlledApplication CTLAPP => UICTLAPP?.ControlledApplication;

        /// <summary>
        /// The active UIDocument for the session.
        /// </summary>
        public static UIDocument UIDOC => UIAPP?.ActiveUIDocument;

        /// <summary>
        /// The active Document for the session.
        /// </summary>
        public static Document DOC => UIDOC?.Document;

        /// <summary>
        /// The active View for the session.
        /// </summary>
        public static DB.View ACTIVE_VIEW => UIDOC?.ActiveGraphicalView;

        #endregion

        #region AddIn properties

        /// <summary>
        /// The add-in ID.
        /// </summary>
        public static AddInId ADDIN_ID => CTLAPP?.ActiveAddInId;

        /// <summary>
        /// The add-in GUID.
        /// </summary>
        public static System.Guid ADDIN_GUID => CTLAPP == null ? System.Guid.Empty : CTLAPP.ActiveAddInId.GetGUID();

        /// <summary>
        /// The add-in Assembly.
        /// </summary>
        public static Assembly ADDIN_ASSEMBLY { get; } = Assembly.GetExecutingAssembly();

        /// <summary>
        /// The add-in name.
        /// </summary>
        public static string ADDIN_NAME { get; } = nameof(Oi);

        /// <summary>
        /// The path to the add-in Assembly.
        /// </summary>
        public static string ADDIN_ASSEMBLY_PATH { get; } = ADDIN_ASSEMBLY.Location;

        /// <summary>
        /// The path to the Dll for the add-in Assembly.
        /// </summary>
        public static string ADDIN_ASSEMBLY_DLLPATH { get; } = Path.GetDirectoryName(ADDIN_ASSEMBLY_PATH);

        /// <summary>
        /// The add-in version number.
        /// </summary>
        public static string ADDIN_VERSION { get; } = GetVersionString(ADDIN_ASSEMBLY);

        private static string GetVersionString(Assembly assembly, string fallback = "99.00.00.00")
        {
            if (assembly?.GetName()?.Version is Version version)
            {
                return $"{version.Major}.{version.Minor:D2}.{version.Build:D2}.{version.Revision:D2}";
            }
            return fallback;
        }

        /// <summary>
        /// The path to the current user's folder.
        /// </summary>
        public static string ADDIN_SETTINGS_FOLDER => Path.Combine(WINDOWS_PATH_APPDATA, ADDIN_NAME);

        /// <summary>
        /// The path to the current user's folder.
        /// </summary>
        public static string ADDIN_SETTINGS_FILE => Path.Combine(ADDIN_SETTINGS_FOLDER, $"{ADDIN_NAME}_Settings.txt");

        #endregion

        #region Revit properties

        /// <summary>
        /// The full Revit version as a string.
        /// </summary>
        public static string REVIT_VERSION_STR => UICTLAPP?.ControlledApplication.VersionNumber;

        /// <summary>
        /// The major Revit version as an integer.
        /// </summary>
        public static int REVIT_VERSION_INT => REVIT_VERSION_STR.Ext_ToIntWithFallback();

        /// <summary>
        /// The Revit username of the current user.
        /// </summary>
        public static string REVIT_USERNAME => UIAPP?.Application.Username;

        #endregion

        #region Windows properties

        /// <summary>
        /// The Windows username of the current user.
        /// </summary>
        public static string WINDOWS_USERNAME { get; } = Environment.UserName;

        /// <summary>
        /// The Windows machinename of the current user.
        /// </summary>
        public static string WINDOWS_MACHINENAME { get; } = Environment.MachineName;

        /// <summary>
        /// The path to the current user's folder.
        /// </summary>
        public static string WINDOWS_PATH_USERFOLDER { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
       
        /// <summary>
        /// The path to local AppData.
        /// </summary>
        public static string WINDOWS_PATH_APPDATA { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// The path to ProgramData.
        /// </summary>
        public static string WINDOWS_PATH_PROGRAMDATA { get; } = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        #endregion

        #region Register tooltips

        /// <summary>
        /// A dictionary containing all tooltips by command key.
        /// </summary>
        public static Dictionary<string, string> DICT_TOOLTIPS { get; } = new();

        /// <summary>
        /// Sets up the Global tooltips dictionary.
        /// </summary>
        /// <param name="resourcePath">The full path to the tooltip resource.</param>
        public static void RegisterTooltips(string resourcePath)
        {
            // Construct the assembly, resource and sub-assembly paths
            var resourceManager = new ResourceManager(resourcePath, ADDIN_ASSEMBLY);

            // Get all tooltip entries, store globally
            using (ResourceSet resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true))
            {
                foreach (DictionaryEntry entry in resourceSet)
                {
                    string key = entry.Key.ToString();
                    DICT_TOOLTIPS[key] = entry.Value.ToString();
                }
            }
        }

        #endregion
    }
}