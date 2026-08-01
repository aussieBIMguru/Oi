// The class belongs to the Forms namespace
namespace Oi.Forms
{
    /// <summary>
    /// Static wrapper methods for forms.
    /// </summary>
    public static class FormCallers
    {
        /// <summary>
        /// Returns a message form.
        /// </summary>
        /// <param name="message">Message to display to the user.</param>
        /// <returns>If OK was pressed.</returns>
        public static bool Message(string message)
        {
            return Forms.UserPrompt.ShowMessage(message);
        }

        /// <summary>
        /// Returns a confirmation form.
        /// </summary>
        /// <param name="message">Message to display to the user.</param>
        /// <returns>If Yes was pressed.</returns>
        public static bool MessageYesNo(string message)
        {
            return Forms.UserPrompt.Confirm(message);
        }

        /// <summary>
        /// Displays a protection review form showing protected elements
        /// before allowing the user to proceed with a bypass request.
        /// </summary>
        /// <param name="items">The protected element information to display.</param>
        /// <param name="message">Optional message to user.</param>
        /// <param name="bypassable">Will the task proceed to bypass.</param>
        /// <returns>
        /// <see langword="true"/> if the user made it through bypass.
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool ReviewProtection(List<ProtectionReviewItem> items, string message = null, bool bypassable = true)
        {
            ProtectionReview form = new ProtectionReview(items, message, bypassable);
            return form.ShowDialog() == true && form.ContinueToBypass;
        }

        /// <summary>
        /// Provides a bypass code entry form.
        /// </summary>
        /// <param name="protectedIds">The Elements to bypass.</param>
        /// <returns>If the attempt was successful.</returns>
        public static bool BypassProtection(List<ElementId> protectedIds)
        {
            string challengeCode = Authorization.BuildUserFacingCode(protectedIds);
            return Forms.UserAuthorization.ShowAndAuthorize("Enter bypass code to proceed.", challengeCode);
        }

        /// <summary>
        /// Provides a user code entry form.
        /// </summary>
        public static void GetAdminCode()
        {
            new Forms.UserCodeGenerator().Show();
        }

        /// <summary>
        /// Provides a bypass request entry form.
        /// </summary>
        public static void GetBypassCode()
        {
            new Forms.BypassCodeGenerator().Show();
        }
    }
}