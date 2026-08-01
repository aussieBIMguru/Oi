// The class belongs to the Commands namespace
namespace Oi.Commands.Cmds_Modify
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

            return Result.Succeeded;
        }
    }
}