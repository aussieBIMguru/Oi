// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Methods of this class generally relate to Documents.
    /// </summary>
    public static partial class Document_Ext
    {
        /// <summary>
        /// Attempts to delete an element from the document.
        /// </summary>
        /// <param name="doc">The Document to delete from (extended).</param>
        /// <param name="obj">A Revit Element.</param>
        /// <typeparam name="T">The type of Element to delete.</typeparam>
        /// <returns>A Result object.</returns>
        public static Result Ext_DeleteElement<T>(this Document doc, T obj)
        {
            // Null check
            if (doc == null) { return Result.Failed; }

            // Try to delete the element
            if (obj is Element element)
            {
                try
                {
                    doc.Delete(element.Id);
                    return Result.Succeeded;
                }
                catch
                {
                    // Proceed to final step
                }
            }

            // If we got here, we failed
            return Result.Failed;
        }

        /// <summary>
        /// Creates a new collector object.
        /// </summary>
        /// <param name="doc">A Revit document (extended).</param>
        /// <returns>A FilteredElementCollector object.</returns>
        public static FilteredElementCollector Ext_Collector(this Document doc)
        {
            return new FilteredElementCollector(doc);
        }

        /// <summary>
        /// Creates a new collector object, in the given view.
        /// </summary>
        /// <param name="doc">A Revit document (extended).</param>
        /// <param name="view">An Revit view.</param>
        /// <returns>A FilteredElementCollector object.</returns>
        public static FilteredElementCollector Ext_Collector(this Document doc, DB.View view)
        {
            if (view == null) { return doc.Ext_Collector(); }
            return new FilteredElementCollector(doc, view.Id);
        }

        /// <summary>
        /// Collects all elements (not types) of the provided category.
        /// </summary>
        /// <param name="doc">A Revit document (extended).</param>
        /// <param name="builtInCategory">A Revit BuiltInCategory.</param>
        /// <param name="view">An optional view.</param>
        /// <returns>A list of Elements.</returns>
        public static IEnumerable<Element> Ext_GetElementsOfCategory(this Document doc,
            BuiltInCategory builtInCategory, DB.View view = null)
        {
            return doc.Ext_Collector(view)
                .OfCategory(builtInCategory)
                .WhereElementIsNotElementType();
        }

        /// <summary>
        /// Collects all elements types of the provided category.
        /// </summary>
        /// <param name="doc">A Revit document (extended).</param>
        /// <param name="builtInCategory">A Revit BuiltInCategory.</param>
        /// <param name="view">An optional view.</param>
        /// <returns>A list of Element types.</returns>
        public static IEnumerable<Element> Ext_GetTypesOfCategory(this Document doc,
            BuiltInCategory builtInCategory, DB.View view = null)
        {
            return doc.Ext_Collector(view)
                .OfCategory(builtInCategory)
                .WhereElementIsElementType();
        }

        /// <summary>
        /// Collects all elements of a given class.
        /// </summary>
        /// <typeparam name="T">The class to collect.</typeparam>
        /// <param name="doc">A Revit document (extended).</param>
        /// <param name="view">A Revit view.</param>
        /// <returns>A list of elements of type T.</returns>
        public static IEnumerable<T> Ext_GetElementsOfClass<T>(this Document doc, DB.View view = null)
        {
            return doc.Ext_Collector(view)
                .OfClass(typeof(T))
                .WhereElementIsNotElementType()
                .Cast<T>();
        }

        /// <summary>
        /// Collects all types of a given class.
        /// </summary>
        /// <param name="doc">A Revit document (extended).</param>
        /// <param name="view">A Revit view.</param>
        /// <returns>A list of types of type T.</returns>
        public static IEnumerable<T> Ext_GetTypessOfClass<T>(this Document doc, DB.View view = null)
        {
            return doc.Ext_Collector(view)
                .OfClass(typeof(T))
                .WhereElementIsElementType()
                .Cast<T>();
        }
    }
}