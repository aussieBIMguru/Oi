using System.Windows;

namespace Oi.Forms
{
    /// <summary>
    /// Provides a prompt for collecting a protection reason from the user.
    /// </summary>
    public partial class ProtectionReasonPrompt : Window
    {
        /// <summary>
        /// Indicates whether the user accepted the prompt.
        /// </summary>
        public bool Accepted { get; private set; }


        /// <summary>
        /// Gets the entered protection reason.
        /// </summary>
        public string Reason => ReasonTextBox.Text.Trim();


        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectionReasonPrompt"/> class.
        /// </summary>
        public ProtectionReasonPrompt()
        {
            InitializeComponent();

            Topmost = true;
            ShowInTaskbar = true;

            Loaded += (s, e) => ReasonTextBox.Focus();
        }

        private void ReasonTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AcceptButton.IsEnabled = !string.IsNullOrWhiteSpace(ReasonTextBox.Text);
        }

        private void AcceptButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Accepted = true;

            DialogResult = true;
            Close();
        }


        /// <summary>
        /// Displays the protection reason prompt.
        /// </summary>
        /// <param name="reason">The entered reason, if accepted.</param>
        /// <returns>True if the user accepted the prompt.</returns>
        public static bool Show(
            out string reason)
        {
            var dialog = new ProtectionReasonPrompt();

            bool? result = dialog.ShowDialog();

            reason = dialog.Reason;

            return result == true && dialog.Accepted;
        }
    }
}