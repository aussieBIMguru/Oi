namespace Oi.Protection
{
    /// <summary>
    /// Provides the schema manager configuration for deletion protection.
    ///
    /// This class specializes the generic
    /// <see cref="SchemaManager{TProtectionInfo, TProtectionUpdater}"/>
    /// by supplying:
    ///
    /// - The protection information type used to store and retrieve deletion
    ///   protection metadata.
    /// - The updater responsible for preventing deletion of protected elements.
    /// - The Extensible Storage schema identity.
    /// - The Revit change type monitored by the updater.
    ///
    /// Deletion protection is maintained as a separate protection layer because
    /// Revit treats deletion independently from other element changes. A single
    /// element may therefore have deletion protection, modification protection,
    /// or both applied through separate schemas and updaters.
    /// </summary>
    public class DeleteSchemaManager
        : SchemaManager<Protection.DeleteProtectionInfo, DeleteProtectionUpdater>
    {
        /// <summary>
        /// Creates a new deletion protection schema manager.
        ///
        /// The schema Guid uniquely identifies this Extensible Storage schema
        /// within Revit and must remain unchanged after deployment.
        ///
        /// The updater listens specifically for element deletion events through
        /// <see cref="Autodesk.Revit.DB.Element.GetChangeTypeElementDeletion()"/>,
        /// allowing <see cref="DeleteProtectionUpdater"/> to intercept deletion
        /// attempts involving protected elements.
        /// </summary>
        public DeleteSchemaManager()
            : base(
                "Oi_DeleteProtection",
                "05FD7CCE-4E95-4D02-A80A-54A94334C9A8",
                Element.GetChangeTypeElementDeletion())
        {
        }
    }
}