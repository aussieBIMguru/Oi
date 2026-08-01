using System.Windows;

namespace Oi.Forms
{
    /// <summary>
    /// Provides a reusable message prompt with configurable accept and cancel actions.
    /// </summary>
    public partial class UserPrompt : Window
    {
        /// <summary>
        /// Indicates whether the user accepted the prompt.
        /// </summary>
        public bool Accepted { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="UserPrompt"/> class.
        /// </summary>
        /// <param name="message">Message displayed to the user.</param>
        /// <param name="acceptText">Text displayed on the accept button.</param>
        /// <param name="cancelText">Text displayed on the cancel button.</param>
        /// <param name="showCancel">Whether the cancel button should be displayed.</param>
        public UserPrompt(
            string message,
            string acceptText = "OK",
            string cancelText = "CANCEL",
            bool showCancel = true)
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            MessageTextBlock.Text = message;

            AcceptButton.Content = acceptText;
            CancelButton.Content = cancelText;


            if (!showCancel)
            {
                CancelButton.Visibility = System.Windows.Visibility.Collapsed;
                CancelColumn.Width = new GridLength(0);

                AcceptColumn.Width = new GridLength(1, GridUnitType.Star);
                AcceptButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            }
        }


        /// <summary>
        /// Accepts the prompt.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void AcceptButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Accepted = true;

            DialogResult = true;
            Close();
        }


        /// <summary>
        /// Cancels the prompt.
        /// </summary>
        /// <param name="sender">The button click sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Accepted = false;

            DialogResult = false;
            Close();
        }


        /// <summary>
        /// Displays a standard OK/Cancel prompt.
        /// </summary>
        /// <param name="message">Message displayed to the user.</param>
        /// <returns>
        /// True if the user clicked OK; otherwise false.
        /// </returns>
        public static bool Show(string message)
        {
            var dialog = new UserPrompt(message);

            bool? result = dialog.ShowDialog();

            return result == true && dialog.Accepted;
        }


        /// <summary>
        /// Displays a Yes/No confirmation prompt.
        /// </summary>
        /// <param name="message">Message displayed to the user.</param>
        /// <returns>
        /// True if the user clicked Yes; otherwise false.
        /// </returns>
        public static bool Confirm(string message)
        {
            var dialog = new UserPrompt(
                message,
                "YES",
                "NO");

            bool? result = dialog.ShowDialog();

            return result == true && dialog.Accepted;
        }


        /// <summary>
        /// Displays an information prompt with only an OK button.
        /// </summary>
        /// <param name="message">Message displayed to the user.</param>
        /// <returns>
        /// True when the user clicks OK.
        /// </returns>
        public static bool ShowMessage(string message)
        {
            var dialog = new UserPrompt(
                message,
                "OK",
                "",
                false);

            bool? result = dialog.ShowDialog();

            return result == true && dialog.Accepted;
        }
    }
}