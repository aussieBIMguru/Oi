using System.Windows;
using System.Windows.Media.TextFormatting;

namespace Oi.Forms
{
    /// <summary>
    /// Provides a dialog for generating administrator authorization codes
    /// from Windows user names.
    /// </summary>
    public partial class UserCodeGenerator : Window
    {
        /// <summary>
        /// The message displayed to the user explaining the request.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCodeGenerator"/> class.
        /// </summary>
        public UserCodeGenerator()
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            this.Message = "Enter a Windows username to generate an authorization code. " +
                "The receiving user must then enter it into their settings file in the Oi folder in " +
                "their Local AppData folder and reboot Revit for it to take effect.";

            DataContext = this;
        }


        /// <summary>
        /// Generates an authorization code for the supplied user name.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userName))
            {
                CodeTextBox.Text = "Enter a username.";
                return;
            }

            CodeTextBox.Text = Authorization.BuildUserNameCode(userName);
        }


        /// <summary>
        /// Copies the generated authorization code to the clipboard.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTextBox.Text.Ext_HasNoChars())
            {
                return;
            }

            CodeTextBox.Text.Ext_SendToClipboard();
        }
    }
}