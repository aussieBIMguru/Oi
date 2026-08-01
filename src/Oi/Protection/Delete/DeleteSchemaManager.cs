using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Oi.Documents;

namespace Oi.Schemas
{
    /// <summary>
    /// Manages the Extensible Storage schema used to mark elements as protected
    /// from deletion, and keeps the associated updater triggers synchronized.
    /// </summary>
    public static class DeleteSchemaManager
    {
        /// <summary>
        /// The name assigned to the Extensible Storage schema.
        /// </summary>
        private const string SchemaName = "Oi_DeleteSchema";


        /// <summary>
        /// The unique identifier of the Extensible Storage schema.
        /// </summary>
        private static readonly Guid SchemaGuid =
            new Guid("05FD7CCE-4E95-4D02-A80A-54A94334C9A8");


        /// <summary>
        /// Cached protection information grouped by document and element.
        /// This allows deleted elements to still be reviewed after Revit removes them.
        /// </summary>
        private static readonly Dictionary<Document, Dictionary<ElementId, ProtectionReviewItem>>
            ProtectedElementsByDocument = new();


        /// <summary>
        /// The identifier of the updater responsible for monitoring protected deletions.
        /// </summary>
        private static UpdaterId IUpdaterId;


        /// <summary>
        /// Registers the deletion protection updater and document event handlers.
        /// </summary>
        /// <param name="app">The Revit application instance.</param>
        public static void Register(UIControlledApplication app)
        {
            AddInId addinId = app.ActiveAddInId;


            var updater = new IUpdaters.DeleteIUpdater(addinId);


            UpdaterRegistry.RegisterUpdater(updater);


            IUpdaterId = updater.GetUpdaterId();


            DocumentRegistry.DocumentOpened += OnDocumentOpened;
            DocumentRegistry.DocumentClosing += OnDocumentClosing;


            app.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentChanged;
            app.ControlledApplication.DocumentReloadedLatest += OnDocumentChanged;
        }


        /// <summary>
        /// Unregisters the deletion protection updater and document event handlers.
        /// </summary>
        /// <param name="app">The Revit application instance.</param>
        public static void Unregister(UIControlledApplication app)
        {
            if (IUpdaterId != null)
            {
                UpdaterRegistry.UnregisterUpdater(IUpdaterId);
            }


            DocumentRegistry.DocumentOpened -= OnDocumentOpened;
            DocumentRegistry.DocumentClosing -= OnDocumentClosing;


            app.ControlledApplication.DocumentSynchronizedWithCentral -= OnDocumentChanged;
            app.ControlledApplication.DocumentReloadedLatest -= OnDocumentChanged;


            ProtectedElementsByDocument.Clear();
        }


        /// <summary>
        /// Refreshes the protection cache when a document is opened.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The document opened event arguments.</param>
        private static void OnDocumentOpened(
            object sender,
            DocumentOpenedEventArgs args)
        {
            RefreshCache(args.Document);
        }


        /// <summary>
        /// Removes cached protection information when a document is closed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The document closing event arguments.</param>
        private static void OnDocumentClosing(
            object sender,
            DocumentClosingEventArgs args)
        {
            ProtectedElementsByDocument.Remove(args.Document);

            SyncTriggers();
        }


        /// <summary>
        /// Refreshes cached protection information after document changes that may
        /// affect protection state, such as synchronization or reload operations.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        private static void OnDocumentChanged(
            object sender,
            EventArgs args)
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
        /// Gets the deletion protection schema, creating it if it does not already exist.
        /// </summary>
        /// <returns>The deletion protection schema.</returns>
        public static Schema GetOrCreateSchema()
        {
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                return schema;
            }


            SchemaBuilder builder = new SchemaBuilder(SchemaGuid);


            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);


            builder.SetSchemaName(SchemaName);


            builder.SetDocumentation(
                "Marks an Element as protected from being deleted from the Document.");


            builder.AddSimpleField("ProtectionStatus", typeof(string));
            builder.AddSimpleField("ProtectedBy", typeof(string));
            builder.AddSimpleField("ProtectedOn", typeof(string));
            builder.AddSimpleField("ProtectionReason", typeof(string));


            return builder.Finish();
        }


        /// <summary>
        /// Marks an element as protected from deletion and stores the protection metadata.
        /// </summary>
        /// <param name="element">The element to protect.</param>
        /// <param name="reason">The reason the element is protected.</param>
        /// <param name="protectedBy">The user responsible for applying protection.</param>
        public static void ProtectElement(
            Element element,
            string reason = null,
            string protectedBy = null)
        {
            Schema schema = GetOrCreateSchema();


            reason ??= "Not specified.";
            protectedBy ??= Globals.WINDOWS_USERNAME;


            DateTime protectedDate = DateTime.UtcNow;


            Entity entity = new Entity(schema);


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


            element.SetEntity(entity);


            GetOrCreateProtectionCache(element.Document)[element.Id] =
                new ProtectionReviewItem
                {
                    ElementId = element.Id,
                    ElementName = element.Name,
                    Category = element.Category?.Name ?? "Unknown",

                    Protection = new Protection.DeleteProtectionInfo
                    {
                        IsProtected = true,
                        ProtectedBy = protectedBy,
                        ProtectedOn = protectedDate,
                        Reason = reason
                    }
                };


            SyncTriggers();
        }


        /// <summary>
        /// Removes deletion protection from an element and clears its cached information.
        /// </summary>
        /// <param name="element">The element to remove protection from.</param>
        public static void UnprotectElement(Element element)
        {
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                element.DeleteEntity(schema);
            }


            GetOrCreateProtectionCache(element.Document)
                .Remove(element.Id);


            SyncTriggers();
        }


        /// <summary>
        /// Determines whether an element is currently protected.
        /// </summary>
        public static bool IsProtected(
            Document doc,
            ElementId id)
        {
            return ProtectedElementsByDocument.TryGetValue(doc, out var cache)
                && cache.ContainsKey(id);
        }


        /// <summary>
        /// Gets cached protection review information for specified elements.
        /// </summary>
        public static List<ProtectionReviewItem> GetProtectionReviewItems(
            Document doc,
            IEnumerable<ElementId> ids)
        {
            if (!ProtectedElementsByDocument.TryGetValue(doc, out var cache))
            {
                return new();
            }


            return ids
                .Where(cache.ContainsKey)
                .Select(id => cache[id])
                .OrderBy(x => x.Protection.ProtectedOn)
                .ToList();
        }


        /// <summary>
        /// Rebuilds the protection cache by scanning the document.
        /// </summary>
        public static void RefreshCache(Document doc)
        {
            Dictionary<ElementId, ProtectionReviewItem> cache = new();


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


                    Protection.DeleteProtectionInfo info = new();


                    if (info.TryGetProtection(element))
                    {
                        cache[element.Id] = new ProtectionReviewItem
                        {
                            ElementId = element.Id,
                            ElementName = element.Name,
                            Category = element.Category?.Name ?? "Unknown",
                            Protection = info
                        };
                    }
                }
            }


            ProtectedElementsByDocument[doc] = cache;


            SyncTriggers();
        }


        /// <summary>
        /// Gets all cached protected element identifiers in the specified document.
        /// </summary>
        public static IReadOnlyCollection<ElementId> GetProtectedElementIds(Document doc)
        {
            return ProtectedElementsByDocument.TryGetValue(doc, out var cache)
                ? cache.Keys.ToList()
                : Array.Empty<ElementId>();
        }


        /// <summary>
        /// Gets or creates the protection cache for a document.
        /// </summary>
        private static Dictionary<ElementId, ProtectionReviewItem> GetOrCreateProtectionCache(
            Document doc)
        {
            if (!ProtectedElementsByDocument.TryGetValue(doc, out var cache))
            {
                cache = new Dictionary<ElementId, ProtectionReviewItem>();

                ProtectedElementsByDocument[doc] = cache;
            }

            return cache;
        }


        /// <summary>
        /// Synchronizes deletion updater triggers for all open documents.
        /// </summary>
        private static void SyncTriggers()
        {
            if (IUpdaterId == null)
            {
                return;
            }


            UpdaterRegistry.RemoveAllTriggers(IUpdaterId);


            foreach (Document document in DocumentRegistry.OpenDocuments)
            {
                IReadOnlyCollection<ElementId> ids =
                    GetProtectedElementIds(document);


                if (ids.Count == 0)
                {
                    continue;
                }


                UpdaterRegistry.AddTrigger(
                    IUpdaterId,
                    document,
                    ids.ToList(),
                    Element.GetChangeTypeElementDeletion());
            }
        }
    }
}