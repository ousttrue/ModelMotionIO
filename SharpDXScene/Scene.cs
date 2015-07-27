using Reactive.Bindings;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using System.Diagnostics;
using MMIO;
using System.Reactive;

namespace SharpDXScene
{
    public class Scene
    {
        PerspectiveProjection m_projection;
        public PerspectiveProjection Projection
        {
            get {
                if (m_projection == null)
                {
                    m_projection = new PerspectiveProjection();
                }
                return m_projection;
            }
        }

        OrbitTransformation m_orbitTransformation;
        public OrbitTransformation OrbitTransformation
        {
            get
            {
                if (m_orbitTransformation == null)
                {
                    m_orbitTransformation= new OrbitTransformation();
                }
                return m_orbitTransformation;
            }
        }

        #region Node
        Node<NodeContent> m_root;
        public Node<NodeContent> Root
        {
            get {
                if(m_root==null)
                {
                    m_root = new Node<NodeContent>();
                    Clear();
                }
                return m_root;
            }
        }

        ReactiveProperty<Node<NodeContent>> m_selected;
        public ReactiveProperty<Node<NodeContent>> Selected
        {
            get {
                if (m_selected == null)
                {
                    m_selected = new ReactiveProperty<Node<NodeContent>>();
                    m_selected
                        .Pairwise()
                        .Subscribe(x =>
                        {
                            if (x.OldItem != null)
                            {
                                x.OldItem.Content.IsSelected.Value = false;
                            }
                            if (x.NewItem != null)
                            {
                                x.NewItem.Content.IsSelected.Value = true;
                            }
                        });
                }
                return m_selected;
            }
        }

        ReadOnlyObservableCollection<Node<NodeContent>> m_nodes;
        public ReadOnlyObservableCollection<Node<NodeContent>> Nodes
        {
            get
            {
                if(m_nodes==null)
                {
                    m_nodes = new ReadOnlyObservableCollection<Node<NodeContent>>(Root.Children);
                }
                return  m_nodes;
            }
        }
        #endregion
        #region AddModel
        public class ModelEventArgs : EventArgs
        {
            public Node<NodeContent> Model { get; set; }
        }
        public event EventHandler<ModelEventArgs> ModelAdded;
        void RaiseModelAdded(Node<NodeContent> model)
        {
            var tmp = ModelAdded;
            if (tmp != null)
            {
                tmp(this, new ModelEventArgs { Model = model });
            }
        }

        public void AddModel(Node<NodeContent> node)
        {
            Root.Add(node);
            RaiseModelAdded(node);

            CurrentPose.Subscribe(x => {

                node.ForEach(Transform.Identity, (path, results) =>
                {
                    var it = path.GetEnumerator();
                    it.MoveNext();
                    var current = it.Current;
                    var name = current.Content.Name.Value;
                    if (x != null && x.Values.ContainsKey(name))
                    {
                        var t = x.Values[name];
                        current.Content.KeyFrame.Value = new Transform(t.Translation, t.Rotation);
                    }
                    else
                    {
                        current.Content.KeyFrame.Value = Transform.Identity;
                    }

                    current.Content.WorldTransform = current.Content.LocalTransform * results.First();

                    return current.Content.WorldTransform;
                });
               
                node.Content.RaisePoseSet();
            });
        }
        #endregion

        #region Motion
        ObservableCollection<Motion> m_motions;
        public ObservableCollection<Motion> Motions
        {
            get
            {
                if (m_motions == null)
                {
                    m_motions = new ObservableCollection<Motion>();
                    m_motions.Add(new Motion("none", 30));
                }
                return m_motions;
            }
        }

        ReactiveProperty<Motion> m_activeMotion;
        public ReactiveProperty<Motion> ActiveMotion
        {
            get
            {
                if (m_activeMotion == null)
                {
                    m_activeMotion = new ReactiveProperty<Motion>();
                    CurrentTime
                        .Select(x => ActiveMotion.Value != null ? ActiveMotion.Value.GetPose(x) : null)
                        .Subscribe(x => CurrentPose.Value = x)
                        ;
                }
                return m_activeMotion;
            }
        }

        ReactiveProperty<Pose> m_currentPose;
        public ReactiveProperty<Pose> CurrentPose
        {
            get
            {
                if (m_currentPose == null)
                {
                    m_currentPose = new ReactiveProperty<Pose>();
                }
                return m_currentPose;
            }
        }
        #endregion
        #region LoadMotion
        Transform VmdBoneFrameToKeyFrame(MMIO.Mmd.VmdBoneFrame vmd, Single scale)
        {
            return new Transform(
                new SharpDX.Vector3(vmd.Position.X, vmd.Position.Y, vmd.Position.Z) * scale
                , new SharpDX.Quaternion(vmd.Rotation.X, vmd.Rotation.Y, vmd.Rotation.Z, vmd.Rotation.W));
        }

        public void LoadVmd(Uri uri, Single scale = 1.58f / 20.0f)
        {
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var vmd = MMIO.Mmd.VmdParse.Execute(bytes);

            var motion = new Motion(Path.GetFileName(uri.LocalPath), 30);
            motion.AddRange(
                vmd.BoneFrames
                .ToLookup(x => x.BoneName)
                .Select(x => new Curve(x.Key, x.ToDictionary(
                    y => y.Frame
                    , y => VmdBoneFrameToKeyFrame(y, scale)
                    ))));

            motion.LastFrame = FrameToTimeSpan(vmd.BoneFrames.Max(x => x.Frame), 30);
            Motions.Add(motion);
        }

        static float ToRadians(float degree)
        {
            return (float)(Math.PI * degree / 180.0f);
        }

        static SharpDX.Quaternion inverseQ(SharpDX.Quaternion q, Axis axis)
        {
            var qaxis = q.Axis;
            var qangle = q.Angle;
            switch (axis)
            {
                case Axis.None: return q;
                case Axis.X: return SharpDX.Quaternion.RotationAxis(new SharpDX.Vector3(-qaxis.X, qaxis.Y, qaxis.Z), -qangle);
                case Axis.Y: return SharpDX.Quaternion.RotationAxis(new SharpDX.Vector3(qaxis.X, -qaxis.Y, qaxis.Z), -qangle);
                case Axis.Z: return SharpDX.Quaternion.RotationAxis(new SharpDX.Vector3(qaxis.X, qaxis.Y, -qaxis.Z), -qangle);
            }
            throw new ArgumentException();
        }

        public Transform ToTransform(IEnumerator<Single> it
            , IEnumerable<MMIO.Bvh.ChannelType> channels
            , Single scale, Axis axis, bool yRotate)
        {
            var t = SharpDX.Vector3.Zero;
            var r = SharpDX.Matrix.Identity;
            var axisX = axis == Axis.X ? -1.0f : 1.0f;
            var axisY = axis == Axis.Y ? -1.0f : 1.0f;
            var axisZ = axis == Axis.Z ? -1.0f : 1.0f;
            foreach (var channel in channels)
            {
                it.MoveNext();
                switch (channel)
                {
                    case MMIO.Bvh.ChannelType.Xposition:
                        t.X = it.Current * scale * axisX;
                        break;

                    case MMIO.Bvh.ChannelType.Yposition:
                        t.Y = it.Current * scale * axisY;
                        break;

                    case MMIO.Bvh.ChannelType.Zposition:
                        t.Z = it.Current * scale * axisZ;
                        break;

                    case MMIO.Bvh.ChannelType.Xrotation:
                        r = SharpDX.Matrix.RotationX(ToRadians(it.Current)) * r;
                        break;

                    case MMIO.Bvh.ChannelType.Yrotation:
                        r = SharpDX.Matrix.RotationY(ToRadians(it.Current)) * r;
                        break;

                    case MMIO.Bvh.ChannelType.Zrotation:
                        r = SharpDX.Matrix.RotationZ(ToRadians(it.Current)) * r;
                        break;

                    default:
                        throw new ArgumentException();
                }
            }

            var q = SharpDX.Quaternion.RotationMatrix(r);
            return new Transform(t, inverseQ(q, axis));
        }

        public void LoadBvhMotion(Uri uri, Single scale, Axis flipAxis, bool yRotate)
        {
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, true);

            var motion = new Motion(Path.GetFileName(uri.LocalPath), bvh.Fps);
            foreach (var frame in bvh.Frames)
            {
                var it = ((IEnumerable<Single>)frame).GetEnumerator();
                var pose = new Pose();
                pose.Values = new Dictionary<string, Transform>();
                foreach (var node in bvh.Root.Traverse((node, level) => node).Where(x => x.Name != "EndSite"))
                {
                    pose.Values[node.Name] = ToTransform(it, node.Channels, scale, flipAxis, yRotate);
                }
                if (it.MoveNext())
                {
                    throw new ArgumentException();
                }
                motion.AddPose(pose);
            }
            motion.LastFrame = FrameToTimeSpan(bvh.Frames.Length, bvh.Fps);
            Motions.Add(motion);
        }
        #endregion

        #region Time
        TimeSpan FrameToTimeSpan(int frame, int fps)
        {
            return TimeSpan.FromMilliseconds(frame * (1000 / fps));
        }

        Stopwatch m_stopwatch;
        public Stopwatch Stopwatch
        {
            get
            {
                if (m_stopwatch == null)
                {
                    m_stopwatch = new Stopwatch();
                }
                return m_stopwatch;
            }
        }

        ReactiveProperty<TimeSpan> m_currentTime;
        public ReactiveProperty<TimeSpan> CurrentTime
        {
            get
            {
                if (m_currentTime == null)
                {
                    m_currentTime = new ReactiveProperty<TimeSpan>();
                }
                return m_currentTime;
            }
        }

        public void Rewind()
        {
            Stop();
            Stopwatch.Reset();
            CurrentTime.Value = TimeSpan.Zero;
        }

        IDisposable m_timerSubscription;
        public void Start()
        {
            if (m_timerSubscription != null) return;

            Stopwatch.Start();
            m_timerSubscription = Observable.Interval(TimeSpan.FromMilliseconds(33))
                .Select(_ => Stopwatch.Elapsed)
                .Subscribe(x => CurrentTime.Value = x)
                ;
        }

        public void Stop()
        {
            if (m_timerSubscription == null) return;
            m_timerSubscription.Dispose();
            m_timerSubscription = null;
            Stopwatch.Stop();
        }
        #endregion


        #region Clear
        public event EventHandler Cleared;
        void RaiseCleared()
        {
            var tmp = Cleared;
            if (tmp != null) {
                tmp(this, EventArgs.Empty);
            }
        }
        public void Clear()
        {
            Root.Children.Clear();
        }
        #endregion
    }
}
