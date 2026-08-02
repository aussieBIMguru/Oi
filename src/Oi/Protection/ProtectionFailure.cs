namespace Oi.Protection
{
    /// <summary>
    /// This failure is raised when Elements protected by a Schema are involved.
    /// 
    /// The changes are rolled back, and this Failure is raised by Revit to the user.
    /// </summary>
    public class ProtectionFailure
    {
        /// <summary>
        /// The Guid of the Failure type.
        /// </summary>
        private readonly static Guid Guid = new("61407477-0F18-491B-A774-89B0AB385A63");

        /// <summary>
        /// The Id of the Failure.
        /// </summary>
        public readonly static FailureDefinitionId Id = new(Guid);

        /// <summary>
        /// The Definition that is raised.
        /// </summary>
        public readonly static FailureDefinition Definition = FailureDefinition.CreateFailureDefinition(
            Id, FailureSeverity.Error, "Element(s) are protected.\n\n" +
            "Changes will be rolled back.");
    }
}