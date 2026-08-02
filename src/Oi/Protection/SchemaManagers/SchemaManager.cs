using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace Oi.Protection
{
    /// <summary>
    /// Generic manager responsible for controlling an Extensible Storage schema,
    /// the associated protection metadata, the related IUpdater registration,
    /// and the runtime cache used for efficient protection checks.
    ///
    /// This class abstracts the common behaviour between different protection
    /// systems (for example deletion protection or modification protection),
    /// while allowing each protection type to provide its own:
    /// - Protection information model.
    /// - IUpdater implementation.
    /// - ChangeType trigger.
    /// </summary>
    /// <typeparam name="TProtectionInfo">
    /// The protection information object used to read/write protection metadata.
    /// </typeparam>
    /// <typeparam name="TProtectionUpdater">
    /// The IUpdater implementation responsible for enforcing the protection rule.
    /// </typeparam>
    /// <remarks>
    /// Creates a new schema manager.
    /// </remarks>
    /// <param name="schemaName">The name of the Extensible Storage schema.</param>
    /// <param name="guidString">The Guid string used to identify the schema.</param>
    /// <param name="changeType">The Revit change type monitored by the updater.</param>
    public class SchemaManager<TProtectionInfo, TProtectionUpdater>(string schemaName, string guidString, ChangeType changeType)
        where TProtectionInfo : ProtectionInfo, new()
        where TProtectionUpdater : ProtectionUpdater
    {
        /// <summary>
        /// The name assigned to the Extensible Storage schema.
        ///
        /// Each protection system has its own schema name so that different
        /// protection behaviours can coexist independently.
        /// </summary>
        public string SchemaName { get; } = schemaName;

        /// <summary>
        /// The unique identifier of the Extensible Storage schema.
        ///
        /// Revit uses this Guid internally to identify schemas. It must remain
        /// constant once a schema has been released.
        /// </summary>
        public Guid SchemaGuid { get; } = new Guid(guidString);

        /// <summary>
        /// Cached protection information grouped by Document and ElementId.
        ///
        /// Revit removes deleted elements from the document database before
        /// some events are raised, meaning the Extensible Storage entity may
        /// no longer be accessible. This cache preserves enough information
        /// to allow reporting and review of protected elements involved in
        /// failed operations.
        /// </summary>
        public Dictionary<Document, Dictionary<ElementId, ProtectionReviewItem>> ProtectedElementsCache = [];

        /// <summary>
        /// Elements permitted for bypass by the user on a session basis.
        /// </summary>
        public Dictionary<Document, HashSet<ElementId>> BypassedIds = new();

        /// <summary>
        /// The identifier of the IUpdater responsible for enforcing this
        /// protection system.
        ///
        /// This is populated during registration and used later when
        /// synchronising triggers.
        /// </summary>
        public UpdaterId UpdaterId { get; set; }

        /// <summary>
        /// The Revit change type that causes the associated updater to execute.
        ///
        /// Examples:
        /// - Element deletion.
        /// - Element modification.
        ///
        /// Different protection systems can therefore share the same manager
        /// while responding to different Revit events.
        /// </summary>
        public ChangeType ChangeType { get; } = changeType;

        /// <summary>
        /// Registers the protection updater and subscribes the manager
        /// to document lifecycle events.
        ///
        /// The updater itself is created through a factory because different
        /// protection implementations require different constructors while
        /// sharing the same registration workflow.
        /// </summary>
        /// <param name="app">The Revit application context.</param>
        /// <param name="updaterFactory">Factory method used to create the concrete updater.</param>
        public void Register(UIControlledApplication app, Func<AddInId, TProtectionUpdater> updaterFactory)
        {
            AddInId addinId = app.ActiveAddInId;

            // Create and register the concrete protection updater.
            TProtectionUpdater updater = updaterFactory(addinId);

            UpdaterRegistry.RegisterUpdater(updater);

            // Store the identifier so triggers can be managed later.
            UpdaterId = updater.GetUpdaterId();


            // Subscribe to document lifecycle events so the cache remains
            // synchronized with the current Revit session.
            DocumentRegistry.DocumentOpened += OnDocumentOpened;
            DocumentRegistry.DocumentCreated += OnDocumentCreated;
            DocumentRegistry.DocumentClosing += OnDocumentClosing;

            app.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentChanged;
            app.ControlledApplication.DocumentReloadedLatest += OnDocumentChanged;
        }

        /// <summary>
        /// Removes the updater registration, event subscriptions,
        /// and clears all cached protection information.
        /// </summary>
        /// <param name="app">The Revit application context.</param>
        public void Unregister(UIControlledApplication app)
        {
            if (UpdaterId != null)
            {
                UpdaterRegistry.UnregisterUpdater(UpdaterId);
            }

            // Remove document lifecycle event subscriptions.
            DocumentRegistry.DocumentOpened -= OnDocumentOpened;
            DocumentRegistry.DocumentCreated -= OnDocumentCreated;
            DocumentRegistry.DocumentClosing -= OnDocumentClosing;

            app.ControlledApplication.DocumentSynchronizedWithCentral -= OnDocumentChanged;
            app.ControlledApplication.DocumentReloadedLatest -= OnDocumentChanged;

            // Remove all cached information when the protection system shuts down.
            ProtectedElementsCache.Clear();
        }

        /// <summary>
        /// Refreshes the protection cache when a document is opened.
        ///
        /// This ensures protection information stored in Extensible Storage
        /// is loaded into memory before any updater activity occurs.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The document opened event arguments.</param>
        private void OnDocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            RefreshCache(args.Document);
        }

        /// <summary>
        /// Refreshes the protection cache when a new document is created.
        ///
        /// Newly created documents will normally have an empty cache, but
        /// running through the same workflow ensures consistency.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The document created event arguments.</param>
        private void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
        {
            RefreshCache(args.Document);
        }

        /// <summary>
        /// Removes cached protection information when a document closes.
        ///
        /// Documents cannot be kept as keys indefinitely because the Revit
        /// document object is no longer valid after closing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The document closing event arguments.</param>
        private void OnDocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            ProtectedElementsCache.Remove(args.Document);

            // Rebuild triggers because the set of open documents has changed.
            RefreshIUpdaterTriggers();
        }

        /// <summary>
        /// Refreshes cached protection information after operations that may
        /// update the document state externally.
        ///
        /// Examples:
        /// - Synchronising with central.
        /// - Reloading latest from central.
        ///
        /// These operations may introduce changes that affect which elements
        /// require active updater triggers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        private void OnDocumentChanged(object sender, EventArgs args)
        {
            switch (args)
            {
                case DocumentSynchronizedWithCentralEventArgs sync:
                    RefreshCache(sync.Document);
                    break;

                case DocumentReloadedLatestEventArgs reload:
                    RefreshCache(reload.Document);
                    break;
            }
        }

        /// <summary>
        /// Gets the Extensible Storage schema for this protection system.
        ///
        /// If the schema does not exist, it is created and configured.
        ///
        /// Schemas are persisted in the Revit file, so creation only occurs
        /// the first time a document requires the schema.
        /// </summary>
        /// <returns>
        /// The existing or newly created schema.
        /// </returns>
        public Schema GetOrCreateSchema()
        {
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                return schema;
            }

            var builder = new SchemaBuilder(SchemaGuid);

            // Allow all users and applications to read/write this schema.
            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);

            builder.SetSchemaName(SchemaName);

            builder.SetDocumentation(
                "Stores protection metadata for protected Revit elements.");


            ConfigureSchema(builder);

            return builder.Finish();
        }

        /// <summary>
        /// Configures the default fields stored by protection schemas.
        ///
        /// Derived schema managers can override this method when additional
        /// metadata is required.
        /// </summary>
        /// <param name="builder">The schema builder used to define fields.</param>
        protected virtual void ConfigureSchema(SchemaBuilder builder)
        {
            builder.AddSimpleField(Fields.Status, typeof(string));
            builder.AddSimpleField(Fields.ProtectedBy, typeof(string));
            builder.AddSimpleField(Fields.ProtectedOn, typeof(string));
            builder.AddSimpleField(Fields.Reason, typeof(string));
        }

        /// <summary>
        /// Protects multiple elements in a batch operation.
        ///
        /// Triggers are refreshed only once after all elements have been
        /// processed. This avoids repeatedly rebuilding updater triggers when
        /// applying protection to many elements.
        /// </summary>
        /// <param name="elements">Elements to protect.</param>
        /// <param name="reason">User supplied explanation for the protection.</param>
        /// <param name="protectedBy">User responsible for applying protection.</param>
        public void ProtectElements(IEnumerable<Element> elements, string reason = null, string protectedBy = null)
        {
            List<Element> elementList = elements.ToList();

            foreach (Element element in elementList)
            {
                ProtectElement(
                    element,
                    reason,
                    protectedBy,
                    false);
            }

            RefreshIUpdaterTriggers();
        }

        /// <summary>
        /// Protects a single element.
        ///
        /// This overload refreshes updater triggers immediately.
        /// For multiple elements use ProtectElements().
        /// </summary>
        /// <param name="element">Element to protect.</param>
        /// <param name="reason">Reason for protection.</param>
        /// <param name="protectedBy">User applying protection.</param>
        public void ProtectElement(Element element, string reason = null, string protectedBy = null)
        {
            ProtectElement(
                element,
                reason,
                protectedBy,
                true);
        }

        /// <summary>
        /// Internal protection implementation.
        ///
        /// Writes the Extensible Storage entity, updates the runtime cache,
        /// and optionally refreshes updater triggers.
        /// </summary>
        /// <param name="element">Element being protected.</param>
        /// <param name="reason">Protection reason.</param>
        /// <param name="protectedBy">User applying protection.</param>
        /// <param name="refreshTriggers">Whether updater triggers should be rebuilt immediately.</param>
        private void ProtectElement(Element element, string reason = null, string protectedBy = null, bool refreshTriggers = false)
        {
            Schema schema = GetOrCreateSchema();


            reason ??= "Not specified.";
            protectedBy ??= Globals.WINDOWS_USERNAME;


            DateTime protectedDate = DateTime.UtcNow;


            var entity = new Entity(schema);


            entity.Set(
                schema.GetField(Fields.Status),
                Status.Protected);

            entity.Set(
                schema.GetField(Fields.ProtectedBy),
                protectedBy);

            entity.Set(
                schema.GetField(Fields.ProtectedOn),
                protectedDate.ToString("O"));

            entity.Set(
                schema.GetField(Fields.Reason),
                reason);

            // Attach protection metadata to the Revit element.
            element.SetEntity(entity);

            // Maintain the runtime cache so the updater does not need to query
            // Extensible Storage repeatedly during operations.
            GetOrCreateProtectionCache(element.Document)[element.Id] =
                new ProtectionReviewItem
                {
                    ElementId = element.Id,
                    ElementName = element.Name,
                    Category = element.Category?.Name ?? "Unknown",

                    Protection = new TProtectionInfo
                    {
                        IsProtected = true,
                        ProtectedBy = protectedBy,
                        ProtectedOn = protectedDate,
                        Reason = reason
                    }
                };

            if (refreshTriggers)
            {
                RefreshIUpdaterTriggers();
            }
        }

        /// <summary>
        /// Removes protection from multiple elements.
        ///
        /// Trigger rebuilding is performed once after all elements are cleared.
        /// </summary>
        public void UnprotectElements(
            IEnumerable<Element> elements)
        {
            List<Element> elementList = elements.ToList();

            foreach (Element element in elementList)
            {
                UnprotectElement(element, false);
            }

            RefreshIUpdaterTriggers();
        }

        /// <summary>
        /// Removes protection from multiple ElementIds.
        ///
        /// Elements that no longer exist are ignored because they cannot have
        /// their Extensible Storage entity removed.
        /// </summary>
        /// <param name="elementIds">Element identifiers to unprotect.</param>
        /// <param name="doc">Related Revit document.</param>
        public void UnprotectElements(IEnumerable<ElementId> elementIds, Document doc)
        {
            List<Element> elements = elementIds
                .Select(id => id.Ext_GetElement<Element>(doc))
                .Where(element => element != null)
                .ToList();

            UnprotectElements(elements);
        }

        /// <summary>
        /// Removes protection from a single element.
        /// </summary>
        public void UnprotectElement(Element element)
        {
            UnprotectElement(element, true);
        }

        /// <summary>
        /// Internal unprotect implementation.
        ///
        /// Removes the Extensible Storage entity and clears the runtime cache.
        /// </summary>
        private void UnprotectElement(Element element, bool refreshTriggers = false)
        {
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                element.DeleteEntity(schema);
            }

            GetOrCreateProtectionCache(element.Document)
                .Remove(element.Id);

            if (refreshTriggers)
            {
                RefreshIUpdaterTriggers();
            }
        }

        /// <summary>
        /// Determines whether a specific element is currently protected.
        ///
        /// This uses the cache rather than querying Extensible Storage because
        /// this method is expected to be called frequently by updaters.
        /// </summary>
        public bool IsProtected(Document doc, ElementId id)
        {
            return ProtectedElementsCache.TryGetValue(doc, out var cache)
                && cache.ContainsKey(id);
        }

        /// <summary>
        /// Adds an element to the session bypass list.
        /// </summary>
        public void AddBypass(Document doc, ElementId id)
        {
            if (!BypassedIds.TryGetValue(doc, out var cache))
            {
                cache = new HashSet<ElementId>();
                BypassedIds.Add(doc, cache);
            }

            cache.Add(id);
        }

        /// <summary>
        /// Adds elements to the session bypass list.
        /// </summary>
        public void AddBypass(Document doc, List<ElementId> ids)
        {
            if (!BypassedIds.TryGetValue(doc, out var cache))
            {
                cache = new HashSet<ElementId>();
                BypassedIds.Add(doc, cache);
            }

            cache.UnionWith(ids);
        }

        /// <summary>
        /// Determines whether a specific element is currently bypassed.
        /// 
        /// This is a per user/model session override only.
        /// </summary>
        public bool HasBypassAuthorization(Document doc, ElementId id)
        {
            return BypassedIds.TryGetValue(doc, out var cache)
                && cache.Contains(id);
        }

        /// <summary>
        /// Retrieves protection information for selected elements.
        /// </summary>
        public List<ProtectionReviewItem> GetProtectionReviewItems(Document doc, IEnumerable<ElementId> ids)
        {
            if (!ProtectedElementsCache.TryGetValue(doc, out var cache))
            {
                return [];
            }

            return ids
                .Where(cache.ContainsKey)
                .Select(id => cache[id])
                .OrderBy(item => item.Protection.ProtectedOn)
                .ToList();
        }

        /// <summary>
        /// Rebuilds the entire protection cache from Extensible Storage.
        ///
        /// This is used when documents open or external changes occur.
        /// </summary>
        public void RefreshCache(Document doc)
        {
            Dictionary<ElementId, ProtectionReviewItem> cache = [];

            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                foreach (Element element in new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType())
                {
                    Entity entity = element.GetEntity(schema);

                    if (!entity.IsValid())
                    {
                        continue;
                    }

                    TProtectionInfo info = new();

                    if (info.TryGetProtection(element))
                    {
                        cache[element.Id] =
                            new ProtectionReviewItem
                            {
                                ElementId = element.Id,
                                ElementName = element.Name,
                                Category = element.Category?.Name ?? "Unknown",
                                Protection = info
                            };
                    }
                }
            }

            ProtectedElementsCache[doc] = cache;

            RefreshIUpdaterTriggers();
        }

        /// <summary>
        /// Returns all protected element identifiers for a document.
        /// </summary>
        public IReadOnlyCollection<ElementId> GetProtectedElementIds(Document doc)
        {
            return ProtectedElementsCache.TryGetValue(doc, out var cache)
                ? cache.Keys.ToList()
                : Array.Empty<ElementId>();
        }

        /// <summary>
        /// Retrieves an existing cache for a document or creates one.
        /// </summary>
        private Dictionary<ElementId, ProtectionReviewItem> GetOrCreateProtectionCache(Document doc)
        {
            if (!ProtectedElementsCache.TryGetValue(doc, out var cache))
            {
                cache = [];

                ProtectedElementsCache[doc] = cache;
            }

            return cache;
        }

        /// <summary>
        /// Rebuilds all updater triggers for every open document.
        ///
        /// Revit updater triggers are not automatically aware of changes to
        /// protected elements, so whenever protection state changes this method
        /// removes all triggers and recreates them from the current cache.
        /// </summary>
        private void RefreshIUpdaterTriggers()
        {
            if (UpdaterId == null)
            {
                return;
            }

            UpdaterRegistry.RemoveAllTriggers(UpdaterId);

            foreach (Document document in DocumentRegistry.OpenDocuments)
            {
                IReadOnlyCollection<ElementId> ids =
                    GetProtectedElementIds(document);


                if (ids.Count == 0)
                {
                    continue;
                }

                UpdaterRegistry.AddTrigger(
                    UpdaterId,
                    document,
                    ids.ToList(),
                    ChangeType);
            }
        }
    }
}