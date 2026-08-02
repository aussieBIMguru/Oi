namespace Oi.Commands.Cmds_Delete
{
    /// <summary>
    /// Selects any protected Elements by the related Schema in the current Document.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_SelectProtectedElements : IExternalCommand
    {
        /// <summary>
        /// Selects all protected Elements in the current Document.
        /// </summary>
        /// <param name="commandData">Command related data.</param>
        /// <param name="message">Command related message.</param>
        /// <param name="elements">Command related elements.</param>
        /// <returns>A Result.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Get the protected Elements
            var protectedIds = Protection.ManagerRegistry.DeleteSchemaManager.GetProtectedElementIds(doc);

            // Early return if none were found
            if (protectedIds.Count == 0)
            {
                FormCallers.Message("No Protected Elements found.");
                return Result.Succeeded;
            }

            // Set selection to protected Elements
            uidoc.Selection.SetElementIds(protectedIds.ToList());
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Launches a detailed review window regarding the nature of protection on selected Elements.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_ProtectionReview : IExternalCommand
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="commandData">Command related data.</param>
        /// <param name="message">Command related message.</param>
        /// <param name="elements">Command related elements.</param>
        /// <returns>A Result.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Get selected Element(s)
            List<ElementId> protectedIds = uidoc.Selection.GetElementIds()
                .Where(id => Protection.ManagerRegistry.DeleteSchemaManager.IsProtected(doc, id))
                .ToList();

            // Early return if no protection
            if (protectedIds.Count == 0)
            {
                Forms.FormCallers.Message($"Selected Element(s) are not protected.");
                return Result.Succeeded;
            }

            // Run the protection review form
            List<ProtectionReviewItem> reviewItems = Protection.ManagerRegistry.DeleteSchemaManager.GetProtectionReviewItems(doc, protectedIds);
            Forms.FormCallers.ReviewProtection(reviewItems, bypassable: false);
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Adds protection to selected Elements.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_ProtectSelectedElements : IExternalCommand
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="commandData">Command related data.</param>
        /// <param name="message">Command related message.</param>
        /// <param name="elements">Command related elements.</param>
        /// <returns>A Result.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Get selected Element(s) without protection
            List<Element> unprotectedElements = uidoc.Ext_SelectedElements()
                .Where(e => !Protection.ManagerRegistry.DeleteSchemaManager.IsProtected(doc, e.Id))
                .ToList();

            // Early return if already protected
            if (unprotectedElements.Count == 0)
            {
                Forms.FormCallers.Message("All Element(s) are already protected.");
                return Result.Succeeded;
            }

            // Get protection reason from user
            if (Forms.FormCallers.ProtectionReason() is not string reason)
            {
                return Result.Cancelled;
            }

            using (var t = new Transaction(doc, "Oi: Protect Elements"))
            {
                t.Start();

                Protection.ManagerRegistry.DeleteSchemaManager.ProtectElements(unprotectedElements,
                    reason: reason ?? "No reason provided.");

                t.Commit();
            }

            // Notify the user of the outcome
            Forms.FormCallers.Message($"Protection added to {unprotectedElements.Count} Element(s).");
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Removes protection from selected Elements.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_UnprotectSelectedElements : IExternalCommand
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="commandData">Command related data.</param>
        /// <param name="message">Command related message.</param>
        /// <param name="elements">Command related elements.</param>
        /// <returns>A Result.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Catch if a non-admin somehow triggered this command
            if (!Authorization.UserIsAdmin())
            {
                Forms.FormCallers.Message("Oi!\n\nWe're not quite sure how you got to this point, but this is an admin tool only.\n\n" +
                    "You ain't an admin as far as I can tell, but hats off for getting to this point.");
                return Result.Succeeded;
            }

            // Get selected Element(s) with protection
            List<Element> protectedElements = uidoc.Ext_SelectedElements()
                .Where(e => Protection.ManagerRegistry.DeleteSchemaManager.IsProtected(doc, e.Id))
                .ToList();

            // Early return if already unprotected
            if (protectedElements.Count == 0)
            {
                Forms.FormCallers.Message("All Element(s) are already unprotected.");
                return Result.Succeeded;
            }

            using (var t = new Transaction(doc, "Oi: Unprotect Elements"))
            {
                t.Start();

                Protection.ManagerRegistry.DeleteSchemaManager.UnprotectElements(protectedElements);

                t.Commit();
            }

            // Notify the user of the outcome
            Forms.FormCallers.Message($"Protection removed from {protectedElements.Count} Element(s).");
            return Result.Succeeded;
        }
    }
}