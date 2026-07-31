namespace Oi.Failures
{
    /// <summary>
    /// This failure is raised when Elements protected by the related ExtensibleStorage Schema are edited.
    /// The transaction is rolled back, and this Failure is raised by Revit.
    /// </summary>
    public class EditFailure
    {
        /// <summary>
        /// The Guid of the Failure type.
        /// </summary>
        private readonly static Guid Guid = new Guid("D6C45D7F-07AF-4698-99FE-22D747624C0E");

        /// <summary>
        /// The Id of the Failure.
        /// </summary>
        private static FailureDefinitionId Id = new FailureDefinitionId(Guid);

        /// <summary>
        /// The Definition that is raised.
        /// </summary>
        public static FailureDefinition Definition = FailureDefinition.CreateFailureDefinition(
            Id, FailureSeverity.Error, "Element(s) are protected from being edited.\n\n" +
            "Changes will be rolled back.");
    }
}