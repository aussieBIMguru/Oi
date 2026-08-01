using Autodesk.Revit.DB.Events;
using Oi.Schemas;

namespace Oi.Documents
{
    /// <summary>
    /// Tracks documents currently open in the Revit session.
    /// 
    /// This is necessary to allow cacheing and re-attaching to Elements with related IUpdater triggers.
    /// </summary>
    public static class DocumentRegistry
    {
        private static readonly HashSet<Document> Documents = new();


        private static EventHandler<DocumentOpenedEventArgs>? _documentOpenedHandler;

        private static EventHandler<DocumentClosingEventArgs>? _documentClosingHandler;


        /// <summary>
        /// Gets all currently open documents.
        /// </summary>
        public static IReadOnlyCollection<Document> OpenDocuments =>
            Documents;


        /// <summary>
        /// Raised when a document opens.
        /// </summary>
        public static event EventHandler<DocumentOpenedEventArgs>? DocumentOpened;


        /// <summary>
        /// Raised when a document closes.
        /// </summary>
        public static event EventHandler<DocumentClosingEventArgs>? DocumentClosing;


        /// <summary>
        /// Registers Revit document lifecycle events.
        /// </summary>
        public static void Register(UIControlledApplication app)
        {
            _documentOpenedHandler = OnDocumentOpened;
            _documentClosingHandler = OnDocumentClosing;


            app.ControlledApplication.DocumentOpened += _documentOpenedHandler;
            app.ControlledApplication.DocumentClosing += _documentClosingHandler;
        }


        /// <summary>
        /// Populates the registry with documents already open.
        /// </summary>
        public static void Initialize(UIApplication uiApp)
        {
            foreach (Document doc in uiApp.Application.Documents)
            {
                Documents.Add(doc);
            }
        }


        /// <summary>
        /// Removes event subscriptions.
        /// </summary>
        public static void Unregister(UIControlledApplication app)
        {
            if (_documentOpenedHandler != null)
            {
                app.ControlledApplication.DocumentOpened -= _documentOpenedHandler;
            }


            if (_documentClosingHandler != null)
            {
                app.ControlledApplication.DocumentClosing -= _documentClosingHandler;
            }


            Documents.Clear();
        }


        private static void OnDocumentOpened(
            object sender,
            DocumentOpenedEventArgs args)
        {
            Documents.Add(args.Document);

            DocumentOpened?.Invoke(sender, args);
        }


        private static void OnDocumentClosing(
            object sender,
            DocumentClosingEventArgs args)
        {
            Documents.Remove(args.Document);

            DocumentClosing?.Invoke(sender, args);
        }
    }
}