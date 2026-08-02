using Autodesk.Revit.DB.ExtensibleStorage;

namespace Oi.Protection
{
    /// <summary>
    /// Provides modification protection information from the modification protection schema.
    /// </summary>
    public class ModifyProtectionInfo : ProtectionInfo
    {
        /// <summary>
        /// Gets the protection type represented by this instance.
        /// </summary>
        public override string ProtectionType => "Modification";

        /// <summary>
        /// Attempts to read modification protection information from an element.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>
        /// <see langword="true"/> if the element contains modification protection
        /// information; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool TryGetProtection(Element element)
        {
            Schema schema = ManagerRegistry.ModifySchemaManager.GetOrCreateSchema();
            return base.GetProtection(element, schema);
        }

        /// <summary>
        /// Gets a user-facing modification protection message.
        /// </summary>
        /// <returns>The modification protection message.</returns>
        public override string GetMessage()
        {
            if (!IsProtected)
            {
                return "This element is not protected from modification.";
            }

            return base.GetMessage();
        }
    }
}