// The class belongs to the Commands namespace
namespace Oi.Commands.Cmds_Delete
{
    /// <summary>
    /// A sample command.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_SelectProtectedElements : IExternalCommand
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

            // Get the protected Elements
            var protectedIds = Schemas.DeleteSchemaManager.GetProtectedElementIds(doc);

            // Early return if none were found
            if (protectedIds.Count == 0)
            {
                FormCallers.Message("No Protected Elements found.");
                return Result.Succeeded;
            }


            uidoc.Selection.SetElementIds(protectedIds.ToList());
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// A command.
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
                .Where(id => Schemas.DeleteSchemaManager.IsProtected(doc, id))
                .ToList();

            // Early return if no protection
            if (!protectedIds.Any())
            {
                Forms.FormCallers.Message($"Selected Element(s) are not protected.");
                return Result.Succeeded;
            }

            // Run the protection review form
            List<ProtectionReviewItem> reviewItems = Schemas.DeleteSchemaManager.GetProtectionReviewItems(doc, protectedIds);
            Forms.FormCallers.ReviewProtection(reviewItems, bypassable: false);
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// A command.
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

            // Get selected Element(s)
            List<Element> selectedElements = uidoc.Ext_SelectedElements();

            // Track newly protected Elements
            int protectedCount = 0;

            // Using a Transaction...
            using (Transaction t = new Transaction(doc, "Oi: Protect Elements"))
            {
                t.Start();

                // For each Element...
                foreach (Element element in selectedElements)
                {
                    // If not protected by the Schema, add protection
                    if (!Schemas.DeleteSchemaManager.IsProtected(doc, element.Id))
                    {
                        Schemas.DeleteSchemaManager.ProtectElement(element);
                        protectedCount++;
                    }
                }

                t.Commit();
            }

            // Notify the user of the outcome
            Forms.FormCallers.Message($"Deletion protection added to {protectedCount} new Element(s).");
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// A command.
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

            // Get selected Element(s)
            List<Element> selectedElements = uidoc.Ext_SelectedElements();

            // Track newly unprotected Elements
            int unprotectedCount = 0;

            // Using a Transaction...
            using (Transaction t = new Transaction(doc, "Oi: Unprotect Elements"))
            {
                t.Start();

                // For each Element...
                foreach (Element element in selectedElements)
                {
                    // If protected by the Schema, remove protection
                    if (Schemas.DeleteSchemaManager.IsProtected(doc, element.Id))
                    {
                        Schemas.DeleteSchemaManager.UnprotectElement(element);
                        unprotectedCount++;
                    }
                }

                t.Commit();
            }

            // Notify the user of the outcome
            Forms.FormCallers.Message($"Deletion protection removed from {unprotectedCount} protected Element(s).");
            return Result.Succeeded;
        }
    }
}