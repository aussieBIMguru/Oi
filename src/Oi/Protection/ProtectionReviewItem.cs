using Oi.Protection;

namespace Oi.Forms
{
    /// <summary>
    /// Represents protection information presented in the protection review UI.
    /// </summary>
    public class ProtectionReviewItem
    {
        /// <summary>
        /// The protected element identifier.
        /// </summary>
        public ElementId ElementId { get; init; }

        /// <summary>
        /// The element display name.
        /// </summary>
        public string ElementName { get; init; }

        /// <summary>
        /// The element category.
        /// </summary>
        public string Category { get; init; }

        /// <summary>
        /// The protection information.
        /// </summary>
        public ProtectionInfo Protection { get; init; }


        /// <summary>
        /// Gets a user-friendly protection description.
        /// </summary>
        public string Message => Protection.GetMessage();
    }
}