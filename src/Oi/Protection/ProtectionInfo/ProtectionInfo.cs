using Autodesk.Revit.DB.ExtensibleStorage;

namespace Oi.Protection
{
    /// <summary>
    /// Provides a common representation and behaviour for element protection
    /// information stored through Revit Extensible Storage.
    /// </summary>
    public abstract class ProtectionInfo
    {
        /// <summary>
        /// Gets the type of protection represented by this instance.
        /// </summary>
        public abstract string ProtectionType { get; }

        /// <summary>
        /// Gets whether the element is currently protected.
        /// </summary>
        public bool IsProtected { get; set; }

        /// <summary>
        /// Gets the user or process that applied the protection.
        /// </summary>
        public string ProtectedBy { get; set; }

        /// <summary>
        /// Gets the date and time when the protection was applied.
        /// </summary>
        public DateTime? ProtectedOn { get; set; }

        /// <summary>
        /// Gets the protection date and time formatted for display.
        /// </summary>
        public string ProtectedOnFormatted =>
            ProtectedOn?.ToString("dd/MM/yy HH:mm:ss") ?? "Unknown";

        /// <summary>
        /// Gets the reason the element was protected.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Attempts to acquire protection information from an element.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>
        /// <see langword="true"/> if protection information was successfully
        /// acquired; otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool TryGetProtection(Element element);

        /// <summary>
        /// Gets a human-readable message describing the current protection state.
        /// </summary>
        /// <returns>The protection status message.</returns>
        public virtual string GetMessage()
        {
            if (!IsProtected)
            {
                return $"This element is not protected ({ProtectionType}).";
            }

            return
                $"{ProtectionType} protection active.\n\n" +
                $"Protected by: {ProtectedBy ?? "Unknown"}\n" +
                $"Protected on: {ProtectedOnFormatted}\n" +
                $"Reason: {Reason ?? "No reason provided."}";
        }
    }
}