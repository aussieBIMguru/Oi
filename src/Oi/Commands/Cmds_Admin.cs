// The class belongs to the Commands namespace
namespace Oi.Commands.Cmds_Admin
{
    /// <summary>
    /// A sample command.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_SelfAppointAsAdmin : IExternalCommand
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
            // Check if user is already appointed
            if (Authorization.UserIsAdmin())
            {
                Forms.FormCallers.Message("You are already an Administrator.");
                return Result.Succeeded;
            }

            // Write the admin file with the expected code, if accessible
            if (UtilFil.FileIsAccessible(Globals.ADDIN_SETTINGS_FILE))
            {
                string adminCode = Authorization.BuildUserNameCode();
                var fileContents = new List<string>() { adminCode, Globals.WINDOWS_USERNAME };
                UtilFil.WriteListToFile(Globals.ADDIN_SETTINGS_FILE, fileContents);
                Forms.FormCallers.Message("Administrator priveleges instated\n\nYou can now also appoint other admins.");
                return Result.Succeeded;
            }
            else
            {
                Forms.FormCallers.Message("Settings file is not editable.");
                return Result.Cancelled;
            }
        }
    }

    /// <summary>
    /// A sample command.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_GetAdminCode : IExternalCommand
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
            Forms.FormCallers.GetAdminCode();
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// A sample command.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_GetBypassCode : IExternalCommand
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
            Forms.FormCallers.GetBypassCode();
            return Result.Succeeded;
        }
    }
}