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
        public static readonly string ZeroDoc = typeof(ZeroDoc).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Project = typeof(Project).FullName;

        /// <summary>
        /// The full name of the availability class.
        /// </summary>
        public static readonly string Selection = typeof(Selection).FullName;
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
}