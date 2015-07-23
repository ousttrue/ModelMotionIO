using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using NLog;
using Reactive.Bindings.Extensions;
using SharpDXScene;
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
    static class Extensions
    {
        public static SharpDX.Vector3 ToSharpDX(this MMIO.Vector3 src, Axis axis)
        {
            switch (axis)
            {
                case Axis.None: return new SharpDX.Vector3(src.X, src.Y, src.Z);
                case Axis.X: return new SharpDX.Vector3(-src.X, src.Y, src.Z);
                case Axis.Y: return new SharpDX.Vector3(src.X, -src.Y, src.Z);
                case Axis.Z: return new SharpDX.Vector3(src.X, src.Y, -src.Z);
            }

            throw new ArgumentException();
        }
    }

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
        SharpDXScene.Scene<NodeValue> m_scene;
        public SharpDXScene.Scene<NodeValue> Scene
        {
            get
            {
                if (m_scene == null)
                {
                    m_scene = new SharpDXScene.Scene<NodeValue>();
                }
                return m_scene;
            }
        }

        public void LoadPmd(Uri uri, Single scale=1.58f/20.0f)
        {
            var root = new Node<NodeValue>(uri.ToString());
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmdParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => NodeValue.CreateNode(x.Name
                , x.Position.ToSharpDX(Axis.None) * scale))
                .ToArray()
                ;

            // build tree
            model.Bones.ForEach((x, i) =>
            {
                var node = nodes[i];
                var parent = x.Parent.HasValue ? nodes[x.Parent.Value] : root;
                node.Content.LocalPosition.Value = node.Content.WorldPosition.Value - parent.Content.WorldPosition.Value;
                parent.Add(node);
            });

            Scene.AddModel(root);
        }

        public void LoadPmx(Uri uri, Single scale = 1.58f / 20.0f)
        {
            var root = new Node<NodeValue>(uri.ToString());
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmxParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => NodeValue.CreateNode(x.Name, x.Position.ToSharpDX(Axis.None) * scale))
                .ToArray()
                ;

            // build tree
            model.Bones.ForEach((x, i) =>
            {
                var node = nodes[i];
                var parent = x.ParentIndex.HasValue ? nodes[x.ParentIndex.Value] : root;
                node.Content.LocalPosition.Value = node.Content.WorldPosition.Value - parent.Content.WorldPosition.Value;
                parent.Add(node);
            });

            Scene.AddModel(root);
        }

        public Node<NodeValue> BuildBvh(MMIO.Bvh.Node bvh, Node<NodeValue> parent, Single scale, Axis flipAxis, bool yRotate)
        {
            var node = NodeValue.CreateNode(bvh.Name, SharpDX.Vector3.Zero, bvh.Offset.ToSharpDX(flipAxis) * scale);
            node.Content.WorldPosition.Value = parent.Content.WorldPosition.Value + node.Content.LocalPosition.Value;
            parent.Add(node);

            foreach (var child in bvh.Children)
            {
                BuildBvh(child, node, scale, flipAxis, yRotate);
            }

            return node;
        }

        public void LoadBvhModel(Uri uri, Single scale, Axis flipAxis, bool yRotate)
        {
            var root = NodeValue.CreateNode(uri.ToString());
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, false);

            BuildBvh(bvh.Root, root, scale, flipAxis, yRotate);

            Scene.AddModel(root);
        }

        public LoadParams LoadBvh(Uri uri)
        {
            // 追加パラメーター
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, false);
            var node = new Node<NodeValue>("bvh");
            BuildBvh(bvh.Root, node, 1.0f, Axis.None, false);
            var maxY = node.Traverse().Max(x => x.Content.WorldPosition.Value.Y);
            var scale = 1.0f;
            while (maxY > 1.0f)
            {
                maxY *= 0.1f;
                scale *= 0.1f;
            }
            while (maxY < 0.1f)
            {
                maxY *= 10.0f;
                scale *= 10.0f;
            }
            return new LoadParams(scale);
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

        #region MotionCommand
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
                AddItem(item);
            }
        }

        void AddItem(Uri item)
        {
            switch (Path.GetExtension(item.LocalPath).ToUpper())
            {
                case ".PMD":
                    LoadPmd(item);
                    break;

                case ".PMX":
                    LoadPmx(item);
                    break;

                case ".VMD":
                    Scene.LoadVmd(item);
                    break;

                case ".BVH":
                    {
                        var param=LoadBvh(item);
                        var vm = new ImportViewModel(param);
                        ImportDialog(vm);
                        if (!vm.IsDone)
                        {
                            Logger.Info("Import canceled");
                            return;
                        }
                        LoadBvhModel(item, param.Scaling.Value, param.FlipAxis.Value, param.YRotate.Value);
                        Scene.LoadBvhMotion(item, param.Scaling.Value, param.FlipAxis.Value, param.YRotate.Value);
                    }
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
