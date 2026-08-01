namespace Oi.IUpdaters
{
    /// <summary>
    /// An <see cref="IUpdater"/> that prevents protected elements from being deleted.
    /// If a protected element is included in a delete operation, the updater posts
    /// a failure to the document, causing the transaction to roll back.
    /// </summary>
    public class DeleteIUpdater : IUpdater
    {
        /// <summary>
        /// The add-in identifier that owns this updater.
        /// </summary>
        private readonly AddInId AddInId;

        /// <summary>
        /// The unique identifier for this updater instance.
        /// </summary>
        private readonly UpdaterId UpdaterId;

        /// <summary>
        /// The Guid for this updater.
        /// </summary>
        private readonly System.Guid UpdaterGuid = new System.Guid("8E97FA0E-28B5-4B43-93ED-FD615307188D");

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteIUpdater"/> class.
        /// </summary>
        /// <param name="addinId">The add-in identifier used to register the updater.</param>
        public DeleteIUpdater(AddInId addinId)
        {
            this.AddInId = addinId;
            this.UpdaterId = new UpdaterId(addinId, UpdaterGuid);
        }

        /// <summary>
        /// Executes when Revit processes a deletion event.
        /// If any deleted elements are marked as protected, a failure is posted
        /// to cancel the deletion transaction.
        /// </summary>
        /// <param name="data">Information about the update event.</param>
        public void Execute(UpdaterData data)
        {
            // Get the Document being affected
            Document doc = data.GetDocument();

            // Get the involved Ids that are protected by the related Schema
            List<ElementId> protectedIds = data.GetDeletedElementIds()
                .Where(id => Schemas.DeleteSchemaManager.IsProtected(doc, id))
                .ToList();

            // If any Elements are protected, raise a Failure (rolls back changes)
            if (protectedIds.Any())
            {
                // Create items for protection review form
                List<ProtectionReviewItem> reviewItems = Schemas.DeleteSchemaManager.GetProtectionReviewItems(doc, protectedIds);

                // Review and bypass succeeded
                if (Forms.FormCallers.ReviewProtection(reviewItems, bypassable: true)
                    && Forms.FormCallers.BypassProtection(protectedIds))
                {
                    // Using a Transaction...
                    using (Transaction t = new Transaction(doc, "Oi: Unprotect Elements"))
                    {
                        t.Start();

                        // For each Element...
                        foreach (ElementId id in protectedIds)
                        {
                            // If protected by the Schema, remove protection
                            if (Schemas.DeleteSchemaManager.IsProtected(doc, id))
                            {
                                Element element = doc.GetElement(id);
                                Schemas.DeleteSchemaManager.UnprotectElement(element);
                            }
                        }

                        t.Commit();
                    }

                    // This will go on to delete the Elements
                    return;
                }

                // Failure handled if bypass not taken successfully
                FailureMessage fm = new FailureMessage(Failures.DeleteFailure.Id);
                fm.SetFailingElements(protectedIds);
                doc.PostFailure(fm);
            }
        }

        /// <summary>
        /// Gets the unique identifier of this updater.
        /// </summary>
        /// <returns>The updater identifier.</returns>
        public UpdaterId GetUpdaterId() => UpdaterId;

        /// <summary>
        /// Gets the display name of this updater.
        /// </summary>
        /// <returns>The updater name.</returns>
        public string GetUpdaterName() => "DeleteIUpdater";

        /// <summary>
        /// Gets a description of the updater's purpose.
        /// </summary>
        /// <returns>A description of the updater.</returns>
        public string GetAdditionalInformation() => "Prevents the deletion of protected Elements.";

        /// <summary>
        /// Gets the priority used by Revit when scheduling this updater.
        /// </summary>
        /// <returns>The updater change priority.</returns>
        public ChangePriority GetChangePriority() => ChangePriority.Annotations;
    }
}