namespace Oi.Protection
{
    /// <summary>
    /// Handles all of the SchemaManagers in the session, and provides
    /// static/ongoing access to them as instances which allows us to
    /// better template them as classes versus if they were all static.
    /// </summary>
    public static class ManagerRegistry
    {
        /// <summary>
        /// The SchemaManager handling deletion protection.
        /// </summary>
        public static DeleteSchemaManager DeleteSchemaManager { get; set; }

        /// <summary>
        /// The SchemaManager handling modify protection.
        /// </summary>
        public static ModifySchemaManager ModifySchemaManager { get; set; }

        /// <summary>
        /// Establish the Schemas for the first time in a session.
        /// Will only create them if they are not assigned yet.
        /// </summary>
        private static void InitializeSchemas()
        {
            DeleteSchemaManager ??= new DeleteSchemaManager();
            ModifySchemaManager ??= new ModifySchemaManager();
            // Add further SchemeManagers here
        }

        /// <summary>
        /// Registers all SchemaManagers.
        /// </summary>
        /// <param name="app">The UIControlledApplication.</param>
        public static void Register(UIControlledApplication app)
        {
            InitializeSchemas();

            DeleteSchemaManager?.Register(app, id => new DeleteProtectionUpdater(id));
            ModifySchemaManager?.Register(app, id => new ModifyProtectionUpdater(id));
            // Add further SchemeManagers here
        }

        /// <summary>
        /// Unregisters all SchemaManagers.
        /// </summary>
        /// <param name="app">The UIControlledApplication.</param>
        public static void Unregister(UIControlledApplication app)
        {
            DeleteSchemaManager?.Unregister(app);
            ModifySchemaManager?.Unregister(app);
            // Add further SchemeManagers here
        }

        /// <summary>
        /// Refreshes all SchemaManager Element/trigger caches.
        /// 
        /// Typically they handle this via the Document registry and
        /// their own event subscribers, but this is used on startup
        /// of the application to ensure caches are not initially stale.
        /// </summary>
        /// <param name="doc">The Document to refresh.</param>
        public static void RefreshCache(Document doc)
        {
            DeleteSchemaManager?.RefreshCache(doc);
            ModifySchemaManager?.RefreshCache(doc);
            // Add further SchemeManagers here
        }
    }
}
