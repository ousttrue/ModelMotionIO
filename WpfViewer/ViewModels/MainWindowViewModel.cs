using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive.Linq;

namespace WpfViewer.ViewModels
{
    class MainWindowViewModel: Livet.ViewModel
    {
        #region Logger
        static Logger Logger
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }       
        #endregion

        #region OpenFileDialog
        ListenerCommand<OpeningFileSelectionMessage> m_openFileDialogCallbackCommand;
        public ListenerCommand<OpeningFileSelectionMessage> OpenFileDialogCallbackCommand
        {
            get
            {
                if (m_openFileDialogCallbackCommand == null)
                {
                    m_openFileDialogCallbackCommand = new ListenerCommand<OpeningFileSelectionMessage>(OnOpenFileDialog, () => true);
                }
                return m_openFileDialogCallbackCommand;
            }
        }
        void OnOpenFileDialog(OpeningFileSelectionMessage m)
        {
            if (m.Response == null)
            {
                Messenger.Raise(new InformationMessage("Cancel", "Error", MessageBoxImage.Error, "Info"));
                return;
            }

            AddItems(m.Response.Select(f => new Uri(f)));
        }
        #endregion

        #region Scene
        ObservableCollection<Models.Node> m_nodes;
        public ObservableCollection<Models.Node> Nodes
        {
            get {
                if (m_nodes == null)
                {
                    m_nodes = new ObservableCollection<Models.Node>();
                }
                return m_nodes;
            }
        }

        void AddItems(IEnumerable<Uri> items)
        {
            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        void AddItem(Uri item)
        {
            switch (Path.GetExtension(item.LocalPath).ToUpper())
            {
                case ".PMD":
                    Logger.Info("AddItem: {0}", item);
                    break;

                default:
                    Logger.Error("UnknownItem: {0}", item);
                    break;
            }
        }

        void ClearItems()
        {
        }
        #endregion

        #region Messages
        ObservableCollection<LogEventInfo> m_messages;
        public ObservableCollection<LogEventInfo> Messages
        {
            get
            {
                if (m_messages == null)
                {
                    m_messages = new ObservableCollection<LogEventInfo>();
                    ObservableMemoryTarget.Instance.LogObservable
                        .ObserveOnDispatcher()
                        .Subscribe(message => {
                            m_messages.Add(message);
                        });
                }
                return m_messages;
            }
        }
        #endregion
    }
}
