using System.Windows;

namespace Oi.Forms
{
    /// <summary>
    /// Displays protected elements before allowing a bypass request.
    /// </summary>
    public partial class ProtectionReview : Window
    {
        /// <summary>
        /// Whether the user wants to continue to bypass.
        /// </summary>
        public bool ContinueToBypass { get; private set; }

        /// <summary>
        /// Creates a new protection review dialog.
        /// </summary>
        /// <param name="items">Protected element summaries.</param>
        /// <param name="message">Summary of the form.</param>
        /// <param name="bypassable">Should the bypass button be active.</param>
        public ProtectionReview(IEnumerable<ProtectionReviewItem> items, string message = null, bool bypassable = true)
        {
            InitializeComponent();

            message ??= "The following elements are protected, to proceed you will need a bypass code.";

            this.BypassButton.IsEnabled = bypassable;

            DataContext = new
            {  
                Items = items,
                Message = message
            };
        }


        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            ContinueToBypass = false;
            DialogResult = false;
        }


        private void Bypass_Click(
            object sender,
            RoutedEventArgs e)
        {
            ContinueToBypass = true;
            DialogResult = true;
        }
    }
}