using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using MMIO;
using NLog;
using Reactive.Bindings.Extensions;
using SharpDXScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using Win32;


namespace MMIO.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Command
        Livet.Commands.ViewModelCommand m_clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (m_clearCommand == null)
                {
                    m_clearCommand = new ViewModelCommand(() =>
                    {
                        Scene.Clear();
                    });
                }
                return m_clearCommand;
            }
        }

        Livet.Commands.ListenerCommand<IEnumerable<Uri>> m_addItemsCommand;
        public ICommand AddItemsCommand
        {
            get {
                if (m_addItemsCommand == null)
                {
                    m_addItemsCommand = new ListenerCommand<IEnumerable<Uri>>(AddItems);
                }
                return m_addItemsCommand;
            }
        }

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

        Livet.Commands.ViewModelCommand m_rewindCommand;
        public ICommand RewindCommand
        {
            get
            {
                if (m_rewindCommand == null)
                {
                    m_rewindCommand = new Livet.Commands.ViewModelCommand(Scene.Rewind);
                }
                return m_rewindCommand;
            }
        }
        Livet.Commands.ViewModelCommand m_startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (m_startCommand == null)
                {
                    m_startCommand = new Livet.Commands.ViewModelCommand(Scene.Start);
                }
                return m_startCommand;
            }
        }
        Livet.Commands.ViewModelCommand m_stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (m_stopCommand == null)
                {
                    m_stopCommand = new Livet.Commands.ViewModelCommand(Scene.Stop);
                }
                return m_stopCommand;
            }
        }
        #endregion

        #region Scene
        SharpDXScene.Scene m_scene;
        public SharpDXScene.Scene Scene
        {
            get
            {
                if (m_scene == null)
                {
                    m_scene = new SharpDXScene.Scene();
                }
                return m_scene;
            }
        }
        #endregion

        #region RenderModel
        Models.RenderModel m_renderModel;
        public Models.RenderModel RenderModel
        {
            get {
                if (m_renderModel == null)
                {
                    m_renderModel = new Models.RenderModel(Scene);
                }
                return m_renderModel;
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

        protected void ImportDialog(ImportViewModel vm)
        {
            Messenger.Raise(new TransitionMessage(typeof(Views.ImportWindow), vm, TransitionMode.Modal, "Import"));
        }

        void AddItems(IEnumerable<Uri> items)
        {
            foreach (var item in items)
            {
                Scene.Load(item);
            }
        }
    }
}
