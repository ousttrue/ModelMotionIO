using Reactive.Bindings;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;


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

        Node m_root;
        public Node Root
        {
            get {
                if(m_root==null)
                {
                    m_root = new Node("__root__");
                    Clear();
                }
                return m_root;
            }
        }

        ReactiveProperty<Node> m_selected;
        public ReactiveProperty<Node> Selected
        {
            get {
                if (m_selected == null)
                {
                    m_selected = new ReactiveProperty<Node>();
                    m_selected
                        .Pairwise()
                        .Subscribe(x =>
                        {
                            if (x.OldItem != null)
                            {
                                x.OldItem.IsSelected.Value = false;
                            }
                            x.NewItem.IsSelected.Value = true;
                        });
                }
                return m_selected;
            }
        }

        public ReadOnlyObservableCollection<Node> Nodes
        {
            get
            {
                return Root.Children;
            }
        }

        #region AddModel
        public class ModelEventArgs: EventArgs
        {
            public Node Model { get; set; }
        }
        public event EventHandler<ModelEventArgs> ModelAdded;
        void RaiseModelAdded(Node model)
        {
            var tmp = ModelAdded;
            if (tmp != null)
            {
                tmp(this, new ModelEventArgs { Model = model });
            }
        }
        void AddModel(Node model)
        {
            Root.AddChild(model);
            RaiseModelAdded(model);
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
            Root.ClearChildren();
        }
        #endregion

        #region SetPose
        public event EventHandler<ModelEventArgs> PoseSet;
        void RaisePoseSet(Node model)
        {
            var tmp = PoseSet;
            if (tmp != null)
            {
                tmp(this, new ModelEventArgs { Model=model});
            }
        }
        public void SetPose(Pose pose)
        {
            foreach (var node in Root.Children)
            {
                node.SetPose(pose);
                RaisePoseSet(node);
            }
        }
        #endregion

        #region Load
        public void LoadPmd(Uri uri, Single scale=1.58f/20.0f)
        {
            var root = new Node(uri.ToString());
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmdParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node(x.Name, x.Position.ToSharpDX(Axis.None) * scale))
                .ToArray()
                ;

            // build tree
            model.Bones.ForEach((x, i) =>
            {
                var node = nodes[i];
                var parent = x.Parent.HasValue ? nodes[x.Parent.Value] : root;
                node.Offset.Value = node.Position.Value - parent.Position.Value;
                parent.AddChild(node);
            });

            AddModel(root);
        }

        public void LoadPmx(Uri uri, Single scale = 1.58f / 20.0f)
        {
            var root = new Node(uri.ToString());
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmxParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node(x.Name, x.Position.ToSharpDX(Axis.None) * scale))
                .ToArray()
                ;

            // build tree
            model.Bones.ForEach((x, i) =>
            {
                var node = nodes[i];
                var parent = x.ParentIndex.HasValue ? nodes[x.ParentIndex.Value] : root;
                node.Offset.Value = node.Position.Value - parent.Position.Value;
                parent.AddChild(node);
            });

            AddModel(root);
        }

        public Node BuildBvh(MMIO.Bvh.Node bvh, Node parent, Single scale, Axis flipAxis, bool yRotate)
        {
            var node = new Node(bvh.Name, SharpDX.Vector3.Zero, bvh.Offset.ToSharpDX(flipAxis) * scale);
            node.Position.Value = parent.Position.Value + node.Offset.Value;
            parent.AddChild(node);

            foreach (var child in bvh.Children)
            {
                BuildBvh(child, node, scale, flipAxis, yRotate);
            }

            return node;
        }

        public void LoadBvh(Uri uri, Single scale, Axis flipAxis, bool yRotate)
        {
            var root = new Node(uri.ToString());
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, false);

            BuildBvh(bvh.Root, root, scale, flipAxis, yRotate);

            AddModel(root);
        }
        #endregion
    }
}
