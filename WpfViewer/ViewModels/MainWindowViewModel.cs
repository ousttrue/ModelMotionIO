using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using NLog;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using Win32;


namespace WpfViewer.ViewModels
{
    class MainWindowViewModel : Livet.ViewModel
    {
        #region Logger
        static Logger Logger
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }
        #endregion

        #region LivetMessage
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
        protected String[] OpenDialog(String title, String filter = "すべてのファイル(*.*)|*.*", bool multiSelect = false)
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
        #endregion

        #region ClearCommand
        Livet.Commands.ViewModelCommand m_clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (m_clearCommand == null)
                {
                    m_clearCommand = new ViewModelCommand(() =>
                    {
                        m_scene.Clear();
                    });
                }
                return m_clearCommand;
            }
        }
        #endregion

        #region OpenFileDialog
        Livet.Commands.ViewModelCommand m_openFileDialogCommand;
        public ICommand OpenFileDialogCommand
        {
            get
            {
                if (m_openFileDialogCommand == null)
                {
                    m_openFileDialogCommand = new ViewModelCommand(() => {
                        var openfiles = OpenDialog("Select model or motion file"
                            , "モデル・モーション(*.PMD;*.PMX;*.VMD;*.VPD;*.BVH)|*.PMD;*.PMX;*.VMD;*.VPD;*.BVH|すべてのファイル(*.*)|*.*"
                            , true);
                        if (openfiles != null)
                        {
                            AddItems(openfiles.Select(x => new Uri(x)));
                        }
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
        Models.Scene m_scene;
        public Models.Scene Scene
        {
            get {
                if (m_scene == null)
                {
                    m_scene = new Models.Scene();
                }
                return m_scene;
            }
        }
        #endregion

        #region AnimationManager
        AnimationViewModel m_animationViewModel;
        public AnimationViewModel AnimationViewModel
        {
            get {
                if (m_animationViewModel == null) {
                    m_animationViewModel = new AnimationViewModel();
                    m_animationViewModel.ActiveMotion.Subscribe(x => Scene.SetMotion(x));
                }
                return m_animationViewModel;
            }
        }
        #endregion

        #region Win32Event
        Subject<Win32EventArgs> m_win32Subject;
        public IObserver<Win32EventArgs> Win32EventObserver
        {
            get
            {
                if (m_win32Subject == null)
                {
                    m_win32Subject = new Subject<Win32EventArgs>();
                    var mouseMove = m_win32Subject.Where(x => x.EventType == WM.WM_MOUSEMOVE);

                    var mouseLeftDown = m_win32Subject.Where(x => x.EventType == WM.WM_LBUTTONDOWN);
                    var mouseLeftUp = m_win32Subject.Where(x => x.EventType == WM.WM_LBUTTONUP);
                    var dragLeft = mouseMove
                        // マウスムーブをマウスダウンまでスキップ。マウスダウン時にマウスをキャプチャ
                        .SkipUntil(mouseLeftDown)
                        // マウスアップが行われるまでTake。マウスアップでマウスのキャプチャをリリース
                        .TakeUntil(mouseLeftUp)
                        ;
                    dragLeft
                        .Pairwise()
                        .Repeat()
                        .Select(x => x.NewItem.Y - x.OldItem.Y)
                        .Select(x => x > 0 ? 1.1f : 0.9f)
                        .Subscribe(x =>
                        {
                            Scene.OrbitTransformation.Dolly(x);
                        });

                    var mouseRightDown = m_win32Subject.Where(x => x.EventType == WM.WM_RBUTTONDOWN);
                    var mouseRightUp = m_win32Subject.Where(x => x.EventType == WM.WM_RBUTTONUP);
                    var dragRight = mouseMove
                        // マウスムーブをマウスダウンまでスキップ。マウスダウン時にマウスをキャプチャ
                        .SkipUntil(mouseRightDown)
                        // マウスアップが行われるまでTake。マウスアップでマウスのキャプチャをリリース
                        .TakeUntil(mouseRightUp)
                        ;
                    dragRight
                        .Pairwise()
                        // Zipの都合上ここで繰り返す
                        .Repeat()
                        // 値の変換
                        .Select(x => new { x = x.OldItem.X - x.NewItem.X, y = x.OldItem.Y - x.NewItem.Y })
                        .Subscribe(x =>
                        {
                            const float factor = 0.01f;
                            Scene.OrbitTransformation.AddYaw(x.x * factor);
                            Scene.OrbitTransformation.AddPitch(x.y * factor);
                        });

                    var mouseMiddleDown = m_win32Subject.Where(x => x.EventType == WM.WM_MBUTTONDOWN);
                    var mouseMiddleUp = m_win32Subject.Where(x => x.EventType == WM.WM_MBUTTONUP);
                    var dragMiddle = mouseMove
                        // マウスムーブをマウスダウンまでスキップ。マウスダウン時にマウスをキャプチャ
                        .SkipUntil(mouseMiddleDown)
                        // マウスアップが行われるまでTake。マウスアップでマウスのキャプチャをリリース
                        .TakeUntil(mouseMiddleUp)
                        ;
                    dragMiddle
                        .Pairwise()
                        // Zipの都合上ここで繰り返す
                        .Repeat()
                        // 値の変換
                        .Select(x => new { x = x.OldItem.X - x.NewItem.X, y = x.OldItem.Y - x.NewItem.Y })
                        .Subscribe(x =>
                        {
                            const float factor = 0.005f;
                            Scene.OrbitTransformation.AddShift(-x.x * factor, x.y * factor);
                        });

                    var mouseWheel = m_win32Subject.Where(x => x.EventType == WM.WM_MOUSEWHEEL)
                        .Select(x => x.Y)
                        .Select(x => x < 0 ? 1.1f : 0.9f)
                        ;
                    mouseWheel.Subscribe(x =>
                    {
                        Scene.OrbitTransformation.Dolly(x);
                    });

                    var size = m_win32Subject.Where(x => x.EventType == WM.WM_SIZE)
                        ;
                    size.Subscribe(x =>
                    {
                        Scene.Projection.TargetSize = new SharpDX.Vector2(x.X, x.Y);
                    });
                }
                return m_win32Subject;
            }
        }
        #endregion

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
                    m_scene.LoadPmd(item);
                    break;

                case ".PMX":
                    m_scene.LoadPmx(item);
                    break;

                case ".VMD":
                    m_animationViewModel.LoadVmd(item);
                    break;

                default:
                    Logger.Error("UnknownItem: {0}", item);
                    break;
            }
        }

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
