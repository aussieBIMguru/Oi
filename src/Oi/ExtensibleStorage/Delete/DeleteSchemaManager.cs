using Autodesk.Revit.DB.ExtensibleStorage;

namespace Oi.Schemas
{
    /// <summary>
    /// Manages the Extensible Storage schema used to mark elements as protected
    /// from deletion, and keeps the associated updater triggers synchronized.
    /// </summary>
    public static class DeleteSchemaManager
    {
        /// <summary>
        /// The name of the Extensible Storage schema.
        /// </summary>
        private const string SchemaName = "Oi_DeleteSchema";

        /// <summary>
        /// The name of the field used to indicate deletion protection.
        /// </summary>
        private const string FieldName = "Oi_DeletePreventField";

        /// <summary>
        /// The value written to the protection field.
        /// </summary>
        private const string FieldValue = "Oi_Prevent";

        /// <summary>
        /// The unique identifier of the Extensible Storage schema.
        /// </summary>
        private static readonly System.Guid SchemaGuid = new System.Guid("05FD7CCE-4E95-4D02-A80A-54A94334C9A8");

        /// <summary>
        /// Cached protected element identifiers, grouped by document.
        /// </summary>
        private static readonly Dictionary<string, HashSet<ElementId>> ProtectedElementIdsByDocument = new();

        /// <summary>
        /// The updater identifier used to manage deletion triggers.
        /// </summary>
        private static UpdaterId IUpdaterId;

        /// <summary>
        /// Gets a unique cache key for a document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The document cache key.</returns>
        private static string DocumentKey(Document doc) => doc.PathName;

        /// <summary>
        /// Registers the deletion protection updater and document event handlers.
        /// </summary>
        /// <param name="app">The Revit application.</param>
        public static void Register(UIControlledApplication app)
        {
            // Create and register the SchemaManager
            AddInId addinId = app.ActiveAddInId;
            var updater = new IUpdaters.DeleteIUpdater(addinId);
            UpdaterRegistry.RegisterUpdater(updater);
            IUpdaterId = updater.GetUpdaterId();

            // Register recaching of Documents when one opens (Schema can only track one Document at once)
            app.ControlledApplication.DocumentOpened += (sender, args) => RefreshCache(args.Document);

            // Register removal of Document from cache when it closes
            app.ControlledApplication.DocumentClosing += (sender, args) =>
            {
                string key = DocumentKey(args.Document);
                ProtectedElementIdsByDocument.Remove(key);
            };
        }

        /// <summary>
        /// Gets the deletion protection schema, creating it if it does not already exist.
        /// </summary>
        /// <returns>The deletion protection schema.</returns>
        public static Schema GetOrCreateSchema()
        {
            // Lookup the Schema by Guid, return it if it exists already
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                return schema;
            }
            // Otherwise, create the Schema
            else
            {
                SchemaBuilder builder = new SchemaBuilder(SchemaGuid);
                builder.SetReadAccessLevel(AccessLevel.Public);
                builder.SetWriteAccessLevel(AccessLevel.Public);
                builder.SetSchemaName(SchemaName);
                builder.SetDocumentation("Marks an Element as protected from being deleted from the Document.");
                builder.AddSimpleField(FieldName, typeof(string));
                return builder.Finish();
            }
        }

        /// <summary>
        /// Marks an element as protected from deletion.
        /// </summary>
        /// <param name="e">The element to protect.</param>
        public static void ProtectElement(Element e)
        {
            // Get the Schema
            Schema schema = GetOrCreateSchema();

            // Flag the Element as protected
            Entity entity = new Entity(schema);
            Field field = schema.GetField(FieldName);
            entity.Set(field, FieldValue);
            e.SetEntity(entity);

            // Give the Element protection
            Document doc = e.Document;
            GetOrCreateElementIdSet(doc).Add(e.Id);
            SyncTriggers(doc);
        }

        /// <summary>
        /// Removes deletion protection from an element.
        /// </summary>
        /// <param name="e">The element to unprotect.</param>
        public static void UnprotectElement(Element e)
        {
            // Remove the Schema from the Element
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                e.DeleteEntity(schema);
            }

            // Remove the Element protection
            Document doc = e.Document;
            GetOrCreateElementIdSet(doc).Remove(e.Id);
            SyncTriggers(doc);
        }

        /// <summary>
        /// Determines whether the specified element is currently protected.
        /// </summary>
        /// <param name="doc">The document containing the element.</param>
        /// <param name="id">The element identifier.</param>
        /// <returns>
        /// <see langword="true"/> if the element is protected; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool IsProtected(Document doc, ElementId id)
        {
            return ProtectedElementIdsByDocument.TryGetValue(DocumentKey(doc), out var set)
                && set.Contains(id);
        }

        /// <summary>
        /// Rebuilds the cache of protected elements for the specified document.
        /// </summary>
        /// <param name="doc">The document to refresh.</param>
        public static void RefreshCache(Document doc)
        {
            var ids = new HashSet<ElementId>();

            // If Schema exists...
            if (Schema.Lookup(SchemaGuid) is Schema schema)
            {
                // Get the protected Elements
                IList<Element> protectedElements = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .Where(e => e.GetEntity(schema) != null && e.GetEntity(schema).Schema != null)
                    .ToList();

                // Add each Id to a hash set
                foreach (Element e in protectedElements)
                {
                    ids.Add(e.Id);
                }
            }

            // Update the Elements and sync the IUpdaters
            ProtectedElementIdsByDocument[DocumentKey(doc)] = ids;
            SyncTriggers(doc);
        }

        /// <summary>
        /// Gets the identifiers of all protected elements in a document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>A read-only collection of protected element identifiers.</returns>
        public static IReadOnlyCollection<ElementId> GetProtectedElementIds(Document doc)
        {
            // Try to get protected Element set, return empty array otherwise
            return ProtectedElementIdsByDocument.TryGetValue(DocumentKey(doc), out var set)
                ? set
                : Array.Empty<ElementId>();
        }

        /// <summary>
        /// Gets the cached protected element set for a document, creating it if necessary.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The protected element identifier set.</returns>
        private static HashSet<ElementId> GetOrCreateElementIdSet(Document doc)
        {
            // Get the key of the Document
            string key = DocumentKey(doc);

            // If the set does not exist, establish it
            if (!ProtectedElementIdsByDocument.TryGetValue(key, out var set))
            {
                set = new HashSet<ElementId>();
                ProtectedElementIdsByDocument[key] = set;
            }

            // Return the existing or newly created set
            return set;
        }

        /// <summary>
        /// Synchronizes the updater triggers with the current protected element set.
        /// </summary>
        /// <param name="doc">The document whose triggers should be synchronized.</param>
        private static void SyncTriggers(Document doc)
        {
            // If there is no IUpdater, do nothing
            if (IUpdaterId == null) { return; }

            // Get the ElementIds that are protected for this Document
            IList<ElementId> protectedIds = GetOrCreateElementIdSet(doc).ToList();

            // Add a deletion trigger for those Elements
            UpdaterRegistry.AddTrigger(
                IUpdaterId,
                doc,
                protectedIds,
                Element.GetChangeTypeElementDeletion());
        }
    }
}