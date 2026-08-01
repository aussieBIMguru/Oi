using Autodesk.Revit.DB.ExtensibleStorage;

namespace Oi.Protection
{
    /// <summary>
    /// Provides deletion protection information from the deletion protection schema.
    /// </summary>
    internal class DeleteProtectionInfo : ProtectionInfo
    {
        /// <summary>
        /// Gets the protection type represented by this instance.
        /// </summary>
        public override string ProtectionType => "Deletion";

        /// <summary>
        /// Attempts to read deletion protection information from an element.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>
        /// <see langword="true"/> if the element contains deletion protection
        /// information; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool TryGetProtection(Element element)
        {
            Schema schema = Schemas.DeleteSchemaManager.GetOrCreateSchema();

            Entity entity = element.GetEntity(schema);

            if (!entity.IsValid())
            {
                return false;
            }

            IsProtected =entity.Get<string>( schema.GetField(Fields.Status)) == Status.Protected;
            ProtectedBy = entity.Get<string>(schema.GetField(Fields.ProtectedBy));
            Reason = entity.Get<string>(schema.GetField(Fields.Reason));
            string date = entity.Get<string>(schema.GetField(Fields.ProtectedOn));

            if (DateTime.TryParse(date, out DateTime parsed))
            {
                ProtectedOn = parsed;
            }

            return true;
        }

        /// <summary>
        /// Gets a user-facing deletion protection message.
        /// </summary>
        /// <returns>The deletion protection message.</returns>
        public override string GetMessage()
        {
            if (!IsProtected)
            {
                return "This element is not protected from deletion.";
            }

            return base.GetMessage();
        }
    }
}