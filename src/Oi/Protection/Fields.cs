namespace Oi.Protection
{
    /// <summary>
    /// Common field names used by Schemas.
    /// </summary>
    internal static class Fields
    {
        /// <summary>
        /// The protection status of an Element.
        /// </summary>
        public const string Status = "ProtectionStatus";

        /// <summary>
        /// Who protected the Element.
        /// </summary>
        public const string ProtectedBy = "ProtectedBy";

        /// <summary>
        /// When was the Element protected.
        /// </summary>
        public const string ProtectedOn = "ProtectedOn";

        /// <summary>
        /// Why was the Element protected.
        /// </summary>
        public const string Reason = "ProtectionReason";
    }
}