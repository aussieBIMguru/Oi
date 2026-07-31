using System.Diagnostics;

// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Extension methods to the RibbonPanel class.
    /// </summary>
    public static class RibbonPanel_Ext
    {
        /// <summary>
        /// Adds a Pushbutton to the panel.
        /// </summary>
        /// <typeparam name="CommandClass">The related Command class.</typeparam>
        /// <param name="ribbonPanel">The RibbonPanel (extended).</param>
        /// <param name="buttonName">The name for the button.</param>
        /// <param name="availability">The availability name.</param>
        /// <param name="suffix">The icon suffix (none by default).</param>
        /// <returns>A Pushbutton object.</returns>
        public static PushButton Ext_AddPushButton<CommandClass>(this RibbonPanel ribbonPanel,
            string buttonName, string availability = "", string suffix = "")
        {
            // Return an error message if panel is null
            if (ribbonPanel == null)
            {
                Debug.WriteLine($"ERROR: {buttonName} not created, ribbonPanel was null.");
                return null;
            }

            // Make pushbuttondata
            PushButtonData pushButtonData = UtilRib.NewPushButtonData<CommandClass>(buttonName);

            // Make pushbutton, add to panel
            if (ribbonPanel.AddItem(pushButtonData) is PushButton pushButton)
            {
                // If provided, set availability
                if (availability != "")
                {
                    pushButton.AvailabilityClassName = availability;
                }

                // Return the PushButton
                return pushButton;
            }
            // Return an error message if it could not be made
            else
            {
                Debug.WriteLine($"ERROR: Button could not be created ({buttonName})");
                return null;
            }
        }

        /// <summary>
        /// Creates a pulldownbutton on a panel.
        /// </summary>
        /// <param name="ribbonPanel">The RibbonPanel (extended).</param>
        /// <param name="buttonName">The displayed name of the pulldown.</param>
        /// <param name="nameSpace">The namespace the button relates to.</param>
        /// <param name="suffix">The icon suffix (none by default).</param>
        /// <returns>A pulldownButton object.</returns>
        public static PulldownButton Ext_AddPulldownButton(this RibbonPanel ribbonPanel, string buttonName,
            string nameSpace, string suffix = "")
        {
            // Return an error message if panel is null
            if (ribbonPanel == null)
            {
                Debug.WriteLine($"ERROR: {buttonName} not created, ribbonPanel was null.");
                return null;
            }

            // Make pulldownButtonData
            PulldownButtonData pulldownButtonData = UtilRib.NewPulldownButtonData(buttonName, nameSpace);

            // Make pulldown, add to panel
            if (ribbonPanel.AddItem(pulldownButtonData) is PulldownButton pulldownButton)
            {
                // Return the pulldown
                return pulldownButton;
            }
            // Return an error message if it could not be made
            else
            {
                Debug.WriteLine($"ERROR: Pulldown could not be created ({buttonName})");
                return null;
            }
        }
    }
}