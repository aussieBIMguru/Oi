using AVNA = Oi.Availability.AvailabilityNames;

// The class belongs to the root namespace
namespace Oi
{
    /// <summary>
    /// Handles the startup and shutdown behavior of the addin.
    /// </summary>
    public class Application : IExternalApplication
    {
        /// <summary>
        /// Runs when the application starts.
        /// 
        /// Handles the following steps:
        /// - Register global variables
        /// - Register the protection system
        /// - Check if user is admin, and if they are...
        /// - Add panels, pulldowns and buttons to a new tab
        /// </summary>
        public Result OnStartup(UIControlledApplication uiCtlApp)
        {
            // Set up UIAPP subscription, register tooltips
            Globals.UICTLAPP = uiCtlApp;
            Globals.UICTLAPP.Idling += OnIdling;
            Globals.RegisterTooltips($"{Globals.ADDIN_NAME}.Resources.Files.Tooltips");

            // Register Failure definitions and Schemas
            // Note: FailureDefinitions have to be defined in startup
            _ = Failures.DeleteFailure.Definition;
            Schemas.DeleteSchemaManager.Register(uiCtlApp);

            // No tools ribbon if the user is not admin approved
            // This is also establishes the addin folder in AppData regardless
            if (!Authorization.UserIsAdmin())
            {
#if DEBUG
                // Notify user they should self appoint themselves
                Forms.FormCallers.Message(message: $"Debug mode is active for {Globals.ADDIN_NAME} for Revit.\n\n" +
                    $"This is your opportunity to self-appoint yourself as an administrator if you haven't already.\n\n" +
                    $"You will not have a chance to do this in release mode.");
#else
                return Result.Succeeded;
#endif
            }

            // Root command namespace
            string admI = $"{Globals.ADDIN_NAME}.Commands.Cmds_Admin";
            uiCtlApp.Ext_AddRibbonTab(Globals.ADDIN_NAME);

            // Add the administator panel
            RibbonPanel panel = uiCtlApp.Ext_AddRibbonPanelToTab(Globals.ADDIN_NAME, "Tools");

            // Admin dropdown
            PulldownButton pullDownAdmin = panel.Ext_AddPulldownButton(buttonName: "Admin", 
                nameSpace: $"{Globals.ADDIN_NAME}.Commands.Cmds_Admin");

#if DEBUG
            pullDownAdmin.Ext_AddPushButton<Commands.Cmds_Admin.Cmd_SelfAppointAsAdmin>(buttonName: "Self-appoint", availability: AVNA.ZeroDoc);
            panel.AddSeparator();
#endif
            pullDownAdmin.Ext_AddPushButton<Commands.Cmds_Admin.Cmd_GetAdminCode>(buttonName: "Admin code", availability: AVNA.ZeroDoc);
            pullDownAdmin.Ext_AddPushButton<Commands.Cmds_Admin.Cmd_GetBypassCode>(buttonName: "Bypass code", availability: AVNA.ZeroDoc);

            // Add deletion protection tools
            PulldownButton pullDownDelete = panel.Ext_AddPulldownButton(buttonName: "Deletion",
                nameSpace: $"{Globals.ADDIN_NAME}.Commands.Cmds_Delete");

            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_SelectProtectedElements>(buttonName: "Select", availability: AVNA.Project);
            panel.AddSeparator();
            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_ProtectSelectedElements>(buttonName: "Protect selected Elements", availability: AVNA.Selection);
            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_UnprotectSelectedElements>(buttonName: "Unprotect selected Elements", availability: AVNA.Selection);

            // STILL TO ADD: EDITING PROTECTION TOOLS

            // Return succeeded
            return Result.Succeeded;
        }

        /// <summary>
        /// Runs when the application shuts down.
        /// 
        /// Handles the following steps:
        /// - Unsubscribe from events if needed
        /// </summary>
        public Result OnShutdown(UIControlledApplication uiCtlApp)
        {
            // Add unsubscribers here

            // Return succeeded
            return Result.Succeeded;
        }

        /// <summary>
        /// Registers the UIApplication as soon as Revit idles.
        /// Unsubscribes the event once it fires once.
        /// </summary>
        /// <param name="sender">The event sender object (the UIApplication).</param>
        /// <param name="e">Event related arguments..</param>
        /// <returns>Void (nothing).</returns>
        private void OnIdling(object sender, UI.Events.IdlingEventArgs e)
        {
            if (sender is UIApplication uiApp)
            {
                Globals.UICTLAPP.Idling -= OnIdling;
                Globals.UIAPP = uiApp;
            }
        }
    }
}