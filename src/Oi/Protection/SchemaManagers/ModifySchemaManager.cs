namespace Oi.Protection
{
    /// <summary>
    /// Provides the schema manager configuration for modification protection.
    ///
    /// This class specializes the generic <see cref="SchemaManager{TProtectionInfo, TProtectionUpdater}"/>
    /// by supplying:
    /// 
    /// - The protection information type used to store and retrieve modification
    ///   protection metadata.
    /// - The updater responsible for enforcing modification protection rules.
    /// - The Extensible Storage schema identity.
    /// - The Revit change type monitored by the updater.
    ///
    /// Modification protection is intentionally managed separately from other
    /// protection systems because Revit treats different change operations
    /// independently. A protected element may therefore have multiple protection
    /// layers applied through separate schemas and updaters.
    /// </summary>
    public class ModifySchemaManager
        : SchemaManager<Protection.ModifyProtectionInfo, ModifyProtectionUpdater>
    {
        /// <summary>
        /// Creates a new modification protection schema manager.
        ///
        /// The schema Guid is permanently tied to this protection system and
        /// must not be changed after release, otherwise Revit will treat it as
        /// a different Extensible Storage schema.
        ///
        /// The updater listens for any element changes through
        /// <see cref="Autodesk.Revit.DB.Element.GetChangeTypeAny()"/>, allowing the related
        /// <see cref="ModifyProtectionUpdater"/> to evaluate whether a protected
        /// element has been modified.
        /// </summary>
        public ModifySchemaManager()
            : base(
                "Oi_ModifyProtection",
                "B905BE09-4581-4332-AC5F-700B36BF60DD",
                Element.GetChangeTypeAny())
        {
        }
    }
}