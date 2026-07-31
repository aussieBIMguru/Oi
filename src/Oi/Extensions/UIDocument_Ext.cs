// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Extension methods to the UIDocument class.
    /// </summary>
    public static class UIDocument_Ext
    {
        /// <summary>
        /// Gets currently selected elements.
        /// </summary>
        /// <param name="uiDoc">The active UIDocument (extended).</param>
        /// <returns>A list of elements.</returns>
        public static List<Element> Ext_SelectedElements(this UIDocument uiDoc)
        {
            // Null check
            if (uiDoc == null) { return new List<Element>(); }

            // Get selected elements
            return uiDoc.Selection.GetElementIds()
                .Select(i => uiDoc.Document.GetElement(i))
                .Where(e => e != null)
                .ToList();
        }

        /// <summary>
        /// Gets currently selected elements of a given type.
        /// </summary>
        /// <typeparam name="T">The type of elements to get.</typeparam>
        /// <param name="uiDoc">The active UIDocument (extended).</param>
        /// <returns>A list of elements.</returns>
        public static List<T> Ext_SelectedElements<T>(this UIDocument uiDoc)
        {
            // Null check
            if (uiDoc == null) { return new List<T>(); }

            // Return selected elements of type
            return uiDoc.Ext_SelectedElements()
                .OfType<T>()
                .Cast<T>()
                .ToList();
        }
    }
}