using AVNA = Oi.Availability.AvailabilityNames;

// Root namespace for the AddIn
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
        /// 
        /// Check if user is admin, and if they are...
        /// - Add panels, pulldowns and buttons to a new tab
        /// </summary>
        public Result OnStartup(UIControlledApplication uiCtlApp)
        {
            // Set up UIAPP subscription, register tooltips
            Globals.UICTLAPP = uiCtlApp;
            Globals.UICTLAPP.Idling += OnIdling;
            Globals.RegisterTooltips($"{Globals.ADDIN_NAME}.Resources.Files.Tooltips");

            // Register protection systems
            _ = Protection.ProtectionFailure.Definition;
            Protection.DocumentRegistry.Register(uiCtlApp);
            Protection.ManagerRegistry.Register(uiCtlApp);

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

            // Add tab and primary panel
            uiCtlApp.Ext_AddRibbonTab(Globals.ADDIN_NAME);
            RibbonPanel panel = uiCtlApp.Ext_AddRibbonPanelToTab(Globals.ADDIN_NAME, "Tools");

            // Admin dropdown and about button
            panel.Ext_AddPushButton<Commands.Cmds_Admin.Cmd_About>(buttonName: "About", availability: AVNA.ZeroDoc);
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

            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_SelectProtectedElements>(buttonName: "Select protected Elements", availability: AVNA.Project);
            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_ProtectionReview>(buttonName: "Review selected Elements", availability: AVNA.Selection);
            pullDownDelete.AddSeparator();
            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_ProtectSelectedElements>(buttonName: "Protect selected Elements", availability: AVNA.Selection);
            pullDownDelete.Ext_AddPushButton<Commands.Cmds_Delete.Cmd_UnprotectSelectedElements>(buttonName: "Unprotect selected Elements", availability: AVNA.Selection);

            // Add modification protection tools
            PulldownButton pullDownModify = panel.Ext_AddPulldownButton(buttonName: "Modify",
                nameSpace: $"{Globals.ADDIN_NAME}.Commands.Cmds_Modify");

            pullDownModify.Ext_AddPushButton<Commands.Cmds_Modify.Cmd_SelectProtectedElements>(buttonName: "Select protected Elements", availability: AVNA.Project);
            pullDownModify.Ext_AddPushButton<Commands.Cmds_Modify.Cmd_ProtectionReview>(buttonName: "Review selected Elements", availability: AVNA.Selection);
            pullDownModify.AddSeparator();
            pullDownModify.Ext_AddPushButton<Commands.Cmds_Modify.Cmd_ProtectSelectedElements>(buttonName: "Protect selected Elements", availability: AVNA.Selection);
            pullDownModify.Ext_AddPushButton<Commands.Cmds_Modify.Cmd_UnprotectSelectedElements>(buttonName: "Unprotect selected Elements", availability: AVNA.Selection);

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
            Protection.ManagerRegistry.Unregister(uiCtlApp);
            Protection.DocumentRegistry.Unregister(uiCtlApp);

            // Return succeeded
            return Result.Succeeded;
        }

        /// <summary>
        /// Fires as soon as Revit is available for the first time.
        /// Unsubscribes the event once so that it fires once only.
        /// </summary>
        /// <param name="sender">The event sender object (the UIApplication).</param>
        /// <param name="e">Event related arguments..</param>
        private void OnIdling(object sender, UI.Events.IdlingEventArgs e)
        {
            if (sender is UIApplication uiApp)
            {
                // Catch UIApplication
                Globals.UICTLAPP.Idling -= OnIdling;
                Globals.UIAPP = uiApp;

                // Initialize the registry of documents currently open in Revit
                // so schema managers can synchronize their updater triggers.
                Protection.DocumentRegistry.Initialize(uiApp);

                // (Re)build protection caches for documents already open before
                // the add-in initialized.
                foreach (Document doc in Protection.DocumentRegistry.OpenDocuments)
                {
                    Protection.ManagerRegistry.RefreshCache(doc);
                }
            }
        }
    }
}