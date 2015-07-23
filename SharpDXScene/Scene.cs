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

namespace SharpDXScene
{
    static class Extensions
    {
        public static SharpDX.Vector3 ToSharpDX(this MMIO.Vector3 src, Axis axis)
        {
            switch(axis)
            {
                case Axis.None: return new SharpDX.Vector3(src.X, src.Y, src.Z);
                case Axis.X: return new SharpDX.Vector3(-src.X, src.Y, src.Z);
                case Axis.Y: return new SharpDX.Vector3(src.X, -src.Y, src.Z);
                case Axis.Z: return new SharpDX.Vector3(src.X, src.Y, -src.Z);
            }

            throw new ArgumentException();
        }
    }

    public enum NodeType
    {
        None,
        Skeleton,
        Mesh,
    }

    public class NodeValue
    {
        public static Node<NodeValue> CreateNode(String name
            , SharpDX.Vector3 worldPosition
            , SharpDX.Vector3 localPosition
            , NodeType nodeType=NodeType.None
            )
        {
            var node = new Node<NodeValue>(new NodeValue());
            node.Content.Name.Value = name;
            node.Content.WorldPosition.Value = worldPosition;
            node.Content.LocalPosition.Value = localPosition;
            node.Content.AttributeType = nodeType;
            return node;
        }
        public static Node<NodeValue> CreateNode(String name
            , SharpDX.Vector3 worldPosition
            )
        {
            return CreateNode(name, worldPosition, SharpDX.Vector3.Zero);
        }
        public static Node<NodeValue> CreateNode(String name)
        {
            return CreateNode(name, SharpDX.Vector3.Zero);
        }

        ReactiveProperty<String> m_name;
        public ReactiveProperty<String> Name
        {
            get
            {
                if (m_name == null)
                {
                    m_name = new ReactiveProperty<string>();
                }
                return m_name;
            }
        }

        ReactiveProperty<SharpDX.Vector3> m_localPosition;
        public ReactiveProperty<SharpDX.Vector3> LocalPosition
        {
            get
            {
                if (m_localPosition == null)
                {
                    m_localPosition = new ReactiveProperty<Vector3>();
                }
                return m_localPosition;
            }
        }

        ReactiveProperty<SharpDX.Vector3> m_worldPosition;
        public ReactiveProperty<SharpDX.Vector3> WorldPosition
        {
            get
            {
                if(m_worldPosition==null)
                {
                    m_worldPosition = new ReactiveProperty<Vector3>();
                }
                return m_worldPosition;
            }
        }

        ReactiveProperty<Boolean> m_isSelected;
        public ReactiveProperty<Boolean> IsSelected
        {
            get
            {
                if (m_isSelected == null)
                {
                    m_isSelected = new ReactiveProperty<bool>();
                }
                return m_isSelected;
            }
        }


        public string Label
        {
            get
            {
                return String.Format("{0}: {1}", AttributeType, Name);
            }
        }

        public NodeType AttributeType
        {
            get;
            set;
        }

        public Uri IconUri
        {
            get
            {
                switch (AttributeType)
                {
                    case NodeType.Skeleton:
                        return new Uri("/Images/skeleton.png", UriKind.Relative);

                    case NodeType.Mesh:
                        return new Uri("/Images/mesh.png", UriKind.Relative);
                }
                return new Uri("/Images/null.png", UriKind.Relative);
            }
        }

        public Motion Motion
        {
            get;
            set;
        }

        ReactiveProperty<Transform> m_keyframe;
        public ReactiveProperty<Transform> KeyFrame
        {
            get {
                if (m_keyframe == null) {
                    m_keyframe = new ReactiveProperty<Transform>();
                }
                return m_keyframe;
            }            
        }

        public Transform WorldTransform;

        public event EventHandler PoseSet;
    }

    /*
    public class Node : Node<NodeValue>
    {
        public Node(String name)
            : this(name, SharpDX.Vector3.Zero, SharpDX.Vector3.Zero)
        { }

        public Node(String name, SharpDX.Vector3 worldPosition)
            : this(name, worldPosition, SharpDX.Vector3.Zero)
        { }

        public Node(String name, SharpDX.Vector3 worldPosition, SharpDX.Vector3 localPosition)
            : base(new NodeValue())
        {
            Content.Name.Value = name;
            Content.LocalPosition.Value = localPosition;
            Content.WorldPosition.Value = worldPosition;
        }

        public ReactiveProperty<String> Name
        {
            get { return Content.Name; }
        }

        public ReactiveProperty<SharpDX.Vector3> LocalPosition
        {
            get { return Content.LocalPosition; }
        }

        public ReactiveProperty<SharpDX.Vector3> WorldPosition
        {
            get { return Content.WorldPosition; }
        }

        public NodeType AttributeType
        {
            get { return Content.AttributeType; }
            set { Content.AttributeType = value; }
        }
    }
    */

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
        Node<NodeValue> m_root;
        public Node<NodeValue> Root
        {
            get {
                if(m_root==null)
                {
                    m_root = NodeValue.CreateNode("__root__");
                    Clear();
                }
                return m_root;
            }
        }

        ReactiveProperty<Node<NodeValue>> m_selected;
        public ReactiveProperty<Node<NodeValue>> Selected
        {
            get {
                if (m_selected == null)
                {
                    m_selected = new ReactiveProperty<Node<NodeValue>>();
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

        ReadOnlyObservableCollection<Node<NodeValue>> m_nodes;
        public ReadOnlyObservableCollection<Node<NodeValue>> Nodes
        {
            get
            {
                if(m_nodes==null)
                {
                    m_nodes = new ReadOnlyObservableCollection<Node<NodeValue>>(Root.Children);
                }
                return  m_nodes;
            }
        }
        #endregion
        #region AddModel
        public class ModelEventArgs : EventArgs
        {
            public Node<NodeValue> Model { get; set; }
        }
        public event EventHandler<ModelEventArgs> ModelAdded;
        void RaiseModelAdded(Node<NodeValue> model)
        {
            var tmp = ModelAdded;
            if (tmp != null)
            {
                tmp(this, new ModelEventArgs { Model = model });
            }
        }
        void AddModel(Node<NodeValue> node)
        {
            Root.Add(node);
            RaiseModelAdded(node);

            CurrentPose.Subscribe(x => {

                //node.SetPose(x);
                
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

        public LoadParams LoadBvh(Uri uri)
        {
            // 追加パラメーター
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, false);
            var node = NodeValue.CreateNode("bvh");
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

        #region Load
        public void LoadPmd(Uri uri, Single scale=1.58f/20.0f)
        {
            var root = NodeValue.CreateNode(uri.ToString());
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmdParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => NodeValue.CreateNode(x.Name, x.Position.ToSharpDX(Axis.None) * scale))
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

            AddModel(root);
        }

        public void LoadPmx(Uri uri, Single scale = 1.58f / 20.0f)
        {
            var root = NodeValue.CreateNode(uri.ToString());
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

            AddModel(root);
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

            AddModel(root);
        }
        #endregion
    }
}
