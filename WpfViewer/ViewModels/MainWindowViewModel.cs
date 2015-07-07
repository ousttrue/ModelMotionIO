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
using System.Windows.Input;

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

        #region InformationMessage
        protected void InfoDialog(String message)
        {
            Messenger.Raise(new InformationMessage(message, "Info", MessageBoxImage.Information, "Info"));
        }

        protected void ErrorDialog(Exception ex)
        {
            Messenger.Raise(new InformationMessage(ex.Message, "Error", MessageBoxImage.Error, "Info"));
        }
        #endregion

        #region ConfirmationMessage
        protected bool ConfirmDialog(String text, String title)
        {
            var message = new ConfirmationMessage(text, title
                        , MessageBoxImage.Question, MessageBoxButton.YesNo, "Confirm");
            Messenger.Raise(message);
            return message.Response.HasValue && message.Response.Value;
        }
        #endregion

        #region OpeningFileSelectionMessage
        protected String[] OpenDialog(String title, bool multiSelect = false)
        {
            return OpenDialog(title, "すべてのファイル(*.*)|*.*", multiSelect);
        }
        protected String[] OpenDialog(String title, String filter= "すべてのファイル(*.*)|*.*", bool multiSelect=false)
        {
            var message = new OpeningFileSelectionMessage("Open")
            {
                Title = title,
                Filter = filter,
                MultiSelect = multiSelect,
            };
            Messenger.Raise(message);
            return message.Response;
        }
        #endregion

        #region SavingFileSelectionMessage
        protected String SaveDialog(String title, string filename)
        {
            var message = new SavingFileSelectionMessage("Save")
            {
                Title = title,
                FileName = String.IsNullOrEmpty(filename) ? "list.txt" : filename,
            };
            Messenger.Raise(message);
            return message.Response != null ? message.Response[0] : null;
        }
        #endregion

        Livet.Commands.ViewModelCommand m_clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if(m_clearCommand==null)
                {
                    m_clearCommand = new ViewModelCommand(() =>
                      {
                          ClearItems();
                      });
                }
                return m_clearCommand;
            }
        }

        #region OpenFileDialog
        Livet.Commands.ViewModelCommand m_openFileDialogCommand;
        public ICommand OpenFileDialogCommand
        {
            get {
                if (m_openFileDialogCommand == null)
                {
                    m_openFileDialogCommand = new ViewModelCommand(() => {
                        var openfiles = OpenDialog("Select model or motion file"
                            , "モデル・モーション(*.PMD;*.PMX;*.VMD;*.VPD;*.BVH)|*.PMD;*.PMX;*.VMD;*.VPD;*.BVH|すべてのファイル(*.*)|*.*"
                            , true);
                        AddItems(openfiles.Select(x => new Uri(x)));
                    });
                }
                return m_openFileDialogCommand;
            }
        }

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
        Models.Scene m_scene = new Models.Scene();

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
                    Nodes.Add(m_scene.LoadPmd(item));
                    break;

                case ".PMX":
                    Nodes.Add(m_scene.LoadPmx(item));
                    break;

                default:
                    Logger.Error("UnknownItem: {0}", item);
                    break;
            }
        }

        void ClearItems()
        {
            Nodes.Clear();
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
