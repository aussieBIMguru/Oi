namespace Oi.Commands.Cmds_Admin
{
    /// <summary>
    /// Provides information about Oi.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Cmd_About : IExternalCommand
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
            string formMessage = $"Oi!\n\nThanks for using {Globals.ADDIN_NAME} for Revit!\n\n" +
                $"This is a simple set of tools that introduce Element deletion and " +
                $"modification systems, as well as an admin system to manage and bypass it.\n\n" +
                $"This tool is produced under an MIT license, with source code available on github.\n\n" +
                $"Would you like to open the Github page?";
            
            if (Forms.FormCallers.MessageYesNo(formMessage))
            {
                UtilFil.OpenLinkPath(@"https://github.com/aussieBIMguru/Oi");
            }

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Allows someone to self appoint themselves in Debug mode.
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
    /// Runs the form to get an admin appointment code.
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
    /// Runs the form to get a bypass code from a request code.
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