namespace Oi.Protection
{
    /// <summary>
    /// An <see cref="IUpdater"/> that prevents protected elements.
    /// 
    /// If a protected element is included in a related operation, a process
    /// specific to that SchemeManager will be undertaken to determine how
    /// to handle the changes (typically a bypass or rollback).
    /// </summary>
    public abstract class ProtectionUpdater : IUpdater
    {
        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="addInId">The Id of the related AddIn.</param>
        /// <param name="updaterGuid">The Guid of the Updater.</param>
        protected ProtectionUpdater(AddInId addInId, Guid updaterGuid)
        {
            AddInId = addInId;
            UpdaterGuid = updaterGuid;
            UpdaterId = new UpdaterId(addInId, updaterGuid);
        }

        /// <summary>
        /// The Add-In identifier that owns this Updater.
        /// </summary>
        protected AddInId AddInId { get; }

        /// <summary>
        /// The unique identifier for this Updater.
        /// </summary>
        protected UpdaterId UpdaterId { get; }

        /// <summary>
        /// The Guid for this Updater.
        /// </summary>
        protected Guid UpdaterGuid { get; }

        /// <summary>
        /// Executes when Revit processes a related updater event.
        /// 
        /// Behavior is defined at the specific Updater's level.
        /// </summary>
        /// <param name="data">Information about the update event.</param>
        public abstract void Execute(UpdaterData data);

        /// <summary>
        /// Gets the unique identifier of this Updater.
        /// </summary>
        /// <returns>The updater identifier.</returns>
        public UpdaterId GetUpdaterId() => UpdaterId;

        /// <summary>
        /// Gets the display name of this Updater.
        /// </summary>
        /// <returns>The updater name.</returns>
        public virtual string GetUpdaterName() => GetType().Name;

        /// <summary>
        /// Gets a description of the Updater's purpose.
        /// </summary>
        /// <returns>A description of the Updater.</returns>
        public virtual string GetAdditionalInformation() => "Protects Elements from unwanted changes.";

        /// <summary>
        /// Gets the priority used by Revit when scheduling this Updater.
        /// </summary>
        /// <returns>The Updater change priority.</returns>
        public virtual ChangePriority GetChangePriority() => ChangePriority.Annotations;
    }
}