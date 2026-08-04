namespace Oi.Protection
{
    /// <summary>
    /// An <see cref="IUpdater"/> that prevents protected elements from being deleted.
    /// 
    /// If a protected element is included in a delete operation, a bypass opportunity
    /// will be available to the user. If this is failed, the change will be rolled back.
    /// </summary>
    public class DeleteProtectionUpdater : ProtectionUpdater, IUpdater
    {
        /// <summary>
        /// The Guid for this Updater.
        /// </summary>
        public static readonly System.Guid Guid = new("8E97FA0E-28B5-4B43-93ED-FD615307188D");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="addInId">The related AddinIn of the Updater.</param>
        public DeleteProtectionUpdater(AddInId addInId) : base(addInId, Guid) { }

        /// <summary>
        /// Executes when Revit processes a related updater event.
        /// 
        /// In sequence:
        /// - Presence of protected Elements is checked
        /// - A review window for any protected Elements is shown
        /// - The user may attempt to bypass with a code
        /// - If they fail, the change is rolled back
        /// - If they succeed, the change is permitted
        /// </summary>
        /// <param name="data">Information about the update event.</param>
        public override void Execute(UpdaterData data)
        {
            // Get the Document being affected
            Document doc = data.GetDocument();

            // Get the involved Ids that are protected by the related Schema
            List<ElementId> protectedIds = data.GetDeletedElementIds()
                .Where(id => ManagerRegistry.DeleteSchemaManager.IsProtected(doc, id))
                .Where(id => !ManagerRegistry.DeleteSchemaManager.HasBypassAuthorization(doc, id))
                .ToList();

            // If any Elements are protected...
            if (protectedIds.Count > 0)
            {
                // Create items for protection review form
                List<ProtectionReviewItem> reviewItems = ManagerRegistry.DeleteSchemaManager.GetProtectionReviewItems(doc, protectedIds);

                // Review and bypass succeeded
                if (Forms.FormCallers.ReviewProtection(reviewItems, bypassable: true)
                    && Forms.FormCallers.BypassProtection(protectedIds))
                {
                    // Add the Ids to the bypass for the Schema manager
                    ManagerRegistry.DeleteSchemaManager.AddBypass(doc, protectedIds);

                    // Change goes ahead (deletion)
                }
                else
                {
                    // Failure handled if bypass not taken successfully
                    var fm = new FailureMessage(ProtectionFailure.Id);
                    fm.SetFailingElements(protectedIds);
                    doc.PostFailure(fm);
                }
            }
        }

        /// <summary>
        /// Gets the display name of this Updater.
        /// </summary>
        /// <returns>The updater name.</returns>
        public override string GetUpdaterName() => "DeleteUpdater";

        /// <summary>
        /// Gets a description of the Updater's purpose.
        /// </summary>
        /// <returns>A description of the Updater.</returns>
        public override string GetAdditionalInformation() => "Prevents the deletion of protected Elements.";
    }
}