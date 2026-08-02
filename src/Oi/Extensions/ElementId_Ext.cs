// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Extension methods to the ElementId class.
    /// </summary>
    public static class ElementId_Ext
    {
        /// <summary>
        /// Returns if an ElementId is not null or invalid.
        /// </summary>
        /// <param name="id">The ElementId to check.</param>
        /// <returns>A Boolean.</returns>
        public static bool Ext_IsValid(this DB.ElementId id)
        {
            return id != null && id != DB.ElementId.InvalidElementId;
        }

        /// <summary>
        /// Returns if an ElementId is null or invalid.
        /// </summary>
        /// <param name="id">The ElementId to check.</param>
        /// <returns>A Boolean.</returns>
        public static bool Ext_IsInValid(this DB.ElementId id)
        {
            return id == null || id == DB.ElementId.InvalidElementId;
        }

        /// <summary>
        /// Gets the Element of a given ElementId if it's of the specified type.
        /// </summary>
        /// <typeparam name="T">The Type to check for.</typeparam>
        /// <param name="id">The ElementId to convert to an Element.</param>
        /// <param name="doc">The Document to get the Element from.</param>
        /// <returns>A DB Element.</returns>
        public static T Ext_GetElement<T>(this DB.ElementId id, DB.Document doc) where T : DB.Element
        {
            if (id.Ext_IsValid() && doc != null
                && doc.GetElement(id) is T t)
            {
                return t;
            }
            else
            {
                return default(T);
            }
        }
    }
}
