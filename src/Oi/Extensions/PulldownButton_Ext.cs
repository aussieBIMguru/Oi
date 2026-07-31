using System.Diagnostics;

// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Extension methods to the PulldownButton class.
    /// </summary>
    public static class PulldownButton_Ext
    {
        /// <summary>
        /// Adds a Pushbutton to the pulldown.
        /// </summary>
        /// <typeparam name="CommandClass">The related Command class.</typeparam>
        /// <param name="pulldownButton">The PulldownButton (extended).</param>
        /// <param name="buttonName">The name for the button.</param>
        /// <param name="availability">The availability name.</param>
        /// <param name="suffix">The icon suffix (if any).</param>
        /// <returns>A Pushbutton object.</returns>
        public static PushButton Ext_AddPushButton<CommandClass>(this PulldownButton pulldownButton,
            string buttonName, string availability = "", string suffix = "")
        {
            // Return an error message if the pulldownbutton is null
            if (pulldownButton == null)
            {
                Debug.WriteLine($"ERROR: {buttonName} not created, pulldownButton was null.");
                return null;
            }

            // Make pushbuttondata
            PushButtonData pushButtonData = UtilRib.NewPushButtonData<CommandClass>(buttonName);

            // Make pushbutton, add to panel
            if (pulldownButton.AddPushButton(pushButtonData) is PushButton pushButton)
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
    }
}