// The class belongs to the Availability namespace
namespace Oi.Availability
{
    /// <summary>
    /// Provides a shorter means of referencing availability class names.
    /// </summary>
    public static class AvailabilityNames
    {
        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Disabled = typeof(Disabled).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string ZeroDoc = typeof(ZeroDoc).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Document = typeof(Document).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Project = typeof(Project).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Family = typeof(Family).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Workshared = typeof(Workshared).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Selection = typeof(Selection).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string SelectionIncludesSheets = typeof(SelectionIncludesSheets).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string SelectionOnlySheets = typeof(SelectionOnlySheets).FullName;
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to always be unavailable.
    /// </summary>
    public class Disabled : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            return false;
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to always be available.
    /// </summary>
    public class ZeroDoc : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            return true;
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if there is an active Document.
    /// </summary>
    public class Document : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            return uiApp?.ActiveUIDocument != null;
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if there is an active Project Document.
    /// </summary>
    public class Project : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            if (uiApp.ActiveUIDocument is UIDocument uiDoc)
            {
                return !uiDoc.Document.IsFamilyDocument;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if there is an active Family Document.
    /// </summary>
    public class Family : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            if (uiApp.ActiveUIDocument is UIDocument uiDoc)
            {
                return uiDoc.Document.IsFamilyDocument;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if there is an active Workshared Document.
    /// </summary>
    public class Workshared : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            if (uiApp.ActiveUIDocument is UIDocument uiDoc)
            {
                return uiDoc.Document.IsWorkshared;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if there are Elements in the current selection.
    /// </summary>
    public class Selection : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            if (uiApp.ActiveUIDocument != null)
            {
                return categories.Size > 0;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if Sheets are in the current selection.
    /// </summary>
    public class SelectionIncludesSheets : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            if (uiApp.ActiveUIDocument is UIDocument uiDoc)
            {
                return categories.Contains(Category.GetCategory(uiDoc.Document, BuiltInCategory.OST_Sheets));
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// This class is used by commands to check if they are available in the current context.
    /// This availability tells a command to be available if only Sheets are in the current selection.
    /// </summary>
    public class SelectionOnlySheets : IExternalCommandAvailability
    {
        /// <summary>
        /// Returns if a Command using this is available.
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        /// <param name="categories">Categories of selected elements.</param>
        /// <returns>A Boolean indicating if the command is available.</returns>
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categories)
        {
            if (uiApp.ActiveUIDocument is UIDocument uiDoc)
            {
                if (categories.Size > 1) { return false; } // More than one category not permitted
                return categories.Contains(Category.GetCategory(uiDoc.Document, BuiltInCategory.OST_Sheets));
            }
            else
            {
                return false;
            }
        }
    }
}