using Autodesk.Revit.DB.Events;

namespace Oi.Protection
{
    /// <summary>
    /// Tracks documents currently opened in the Revit session.
    /// 
    /// Whenever you Protect/Unprotect Elements, add new Documents
    /// or Sync/Reload a model, the caches of all SchemaManagers
    /// will be refreshed by the Registry.
    /// </summary>
    public static class DocumentRegistry
    {
        /// <summary>
        /// The opened Documents being registered.
        /// </summary>
        private static readonly HashSet<Document> Documents = [];

        /// <summary>
        /// Handler for Document opening event.
        /// </summary>
        private static EventHandler<DocumentOpenedEventArgs> _documentOpenedHandler;

        /// <summary>
        /// Handler for Document creation event.
        /// </summary>
        private static EventHandler<DocumentCreatedEventArgs> _documentCreatedHandler;

        /// <summary>
        /// Handler for Document closing event.
        /// </summary>
        private static EventHandler<DocumentClosingEventArgs> _documentClosingHandler;

        /// <summary>
        /// Gets all currently open documents.
        /// </summary>
        public static IReadOnlyCollection<Document> OpenDocuments => Documents;

        /// <summary>
        /// Raised when a document opens.
        /// </summary>
        public static event EventHandler<DocumentOpenedEventArgs> DocumentOpened;

        /// <summary>
        /// Raised when a document is created.
        /// </summary>
        public static event EventHandler<DocumentCreatedEventArgs> DocumentCreated;

        /// <summary>
        /// Raised when a document closes.
        /// </summary>
        public static event EventHandler<DocumentClosingEventArgs> DocumentClosing;

        /// <summary>
        /// Gets all opened Documents (typically on Revit startup, if the user
        /// has opened a Document to launch the Application).
        /// </summary>
        /// <param name="uiApp">The UIApplication.</param>
        public static void Initialize(UIApplication uiApp)
        {
            foreach (Document doc in uiApp.Application.Documents)
            {
                Documents.Add(doc);
            }
        }

        /// <summary>
        /// Subscribes Document related event handlers.
        /// </summary>
        /// <param name="app">The UIControlledApplication.</param>
        public static void Register(UIControlledApplication app)
        {
            _documentOpenedHandler = OnDocumentOpened;
            _documentCreatedHandler = OnDocumentCreated;
            _documentClosingHandler = OnDocumentClosing;

            app.ControlledApplication.DocumentOpened += _documentOpenedHandler;
            app.ControlledApplication.DocumentCreated += _documentCreatedHandler;
            app.ControlledApplication.DocumentClosing += _documentClosingHandler;
        }

        /// <summary>
        /// Unsubscribes Document related event handlers, effectively
        /// disabling the Document Registry process.
        /// </summary>
        /// <param name="app">The UIControlledApplication.</param>
        public static void Unregister(UIControlledApplication app)
        {
            if (_documentOpenedHandler != null)
            {
                app.ControlledApplication.DocumentOpened -= _documentOpenedHandler;
            }

            if (_documentCreatedHandler != null)
            {
                app.ControlledApplication.DocumentCreated -= _documentCreatedHandler;
            }

            if (_documentClosingHandler != null)
            {
                app.ControlledApplication.DocumentClosing -= _documentClosingHandler;
            }

            Documents.Clear();
        }

        /// <summary>
        /// Raised when a Document opens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnDocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            Documents.Add(args.Document);

            // Forces re-caching of any SchemeManagers
            DocumentOpened?.Invoke(sender, args);
        }

        /// <summary>
        /// Raised when a Document is created.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
        {
            Documents.Add(args.Document);

            // Forces re-caching of any SchemeManagers
            DocumentCreated?.Invoke(sender, args);
        }

        /// <summary>
        /// Raised when a Document closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnDocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            Documents.Remove(args.Document);

            // Forces re-caching of any SchemeManagers
            DocumentClosing?.Invoke(sender, args);
        }
    }
}