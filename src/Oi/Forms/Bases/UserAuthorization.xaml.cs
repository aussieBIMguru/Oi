using System.Windows;

namespace Oi.Forms
{
    /// <summary>
    /// Provides a dialog window for entering and validating an authorization
    /// response code against a supplied challenge code.
    /// </summary>
    public partial class UserAuthorization : Window
    {
        /// <summary>
        /// The message displayed to the user explaining the authorization request.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The challenge code that the user must provide a response for.
        /// </summary>
        public string UserCode { get; }

        /// <summary>
        /// Indicates whether the entered response was successfully validated
        /// before the user confirmed the dialog.
        /// </summary>
        public bool IsAuthorized { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthorization"/> class.
        /// </summary>
        /// <param name="message">The message displayed to the user.</param>
        /// <param name="challenge">The challenge code requiring authorization.</param>
        public UserAuthorization(string message, string challenge)
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            Message = message;
            UserCode = challenge;

            MessageTextBox.Text = message;
            ChallengeTextBlock.Text = challenge;

            DataContext = this;
        }

        /// <summary>
        /// Copies the challenge code to the clipboard for use when generating
        /// a response code.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserCode.Ext_SendToClipboard();
                StatusTextBlock.Text = "Code copied to clipboard.";
            }
            catch (Exception)
            {
                StatusTextBlock.Text = "Could not copy to clipboard.";
            }
        }

        /// <summary>
        /// Validates the entered response code against the challenge code.
        /// Enables confirmation only when the response is successfully verified.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            string entered = ResponseTextBox.Text;

            if (string.IsNullOrWhiteSpace(entered))
            {
                StatusTextBlock.Text = "Enter a bypass code.";
                IsAuthorized = false;
                OkButton.IsEnabled = false;
                return;
            }

            bool valid = Authorization.VerifyUserBypassCode(UserCode, entered);

            if (valid)
            {
                StatusTextBlock.Text = "Authorized. Click OK to proceed.";
                IsAuthorized = true;
                OkButton.IsEnabled = true;
            }
            else
            {
                StatusTextBlock.Text = "Incorrect bypass code.";
                IsAuthorized = false;
                OkButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Confirms the dialog and returns the authorization result.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = IsAuthorized;
            Close();
        }

        /// <summary>
        /// Displays the authorization dialog and returns whether the user
        /// successfully completed authorization.
        /// </summary>
        /// <param name="message">The message displayed to the user.</param>
        /// <param name="challenge">The challenge code requiring authorization.</param>
        /// <returns>
        /// <see langword="true"/> if authorization succeeded; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool ShowAndAuthorize(string message, string challenge)
        {
            var dialog = new UserAuthorization(message, challenge);

            bool? result = dialog.ShowDialog();
            return result == true && dialog.IsAuthorized;
        }
    }
}