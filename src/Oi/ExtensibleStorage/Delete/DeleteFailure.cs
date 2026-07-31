namespace Oi.Failures
{
    /// <summary>
    /// This failure is raised when Elements protected by the related ExtensibleStorage Schema are deleted.
    /// The transaction is rolled back, and this Failure is raised by Revit.
    /// </summary>
    public class DeleteFailure
    {
        /// <summary>
        /// The Guid of the Failure type.
        /// </summary>
        private readonly static Guid Guid = new Guid("61407477-0F18-491B-A774-89B0AB385A63");

        /// <summary>
        /// The Id of the Failure.
        /// </summary>
        public static FailureDefinitionId Id = new FailureDefinitionId(Guid);

        /// <summary>
        /// The Definition that is raised.
        /// </summary>
        public static FailureDefinition Definition = FailureDefinition.CreateFailureDefinition(
            Id, FailureSeverity.Error, "Element(s) are protected from deletion.\n\n" +
            "Changes will be rolled back.");
    }
}