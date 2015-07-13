﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RenderingPipe.Resources;
using RenderingPipe.Commands;
using System.Runtime.InteropServices;
using SharpDX;

namespace WpfViewer.Models
{
    public class Mesh
    {
        public VertexBufferResource VertexBuffer { get; set; }
        public VertexBufferUpdateCommand VertexBufferUpdate { get; set; }
    }

    public class Node : Livet.NotificationObject
    {
        String m_name;
        public String Name
        {
            get { return m_name; }
            set
            {
                if (m_name == value) return;
                m_name = value;
                RaisePropertyChanged(() => this.Name);
                RaisePropertyChanged(() => this.Label);
            }
        }

        bool m_isSelected;
        public Boolean IsSelected
        {
            get { return m_isSelected; }
            set {
                if (m_isSelected == value) return;
                m_isSelected = value;
                RaisePropertyChanged(() => this.IsSelected);
            }
        }

        public String Label
        {
            get
            {
                if (Curve == null) return Name;

                return String.Format("{0}({1})", Name, Curve.Values.Count());
            }
        }

        /// <summary>
        /// Model原点からの位置
        /// </summary>
        SharpDX.Vector3 m_position;
        public SharpDX.Vector3 Position
        {
            get { return m_position; }
            set
            {
                if (m_position == value) return;
                m_position = value;
                RaisePropertyChanged(() => this.Position);
            }
        }

        /// <summary>
        /// 親ボーンからの位置
        /// </summary>
        SharpDX.Vector3 m_offset;
        public SharpDX.Vector3 Offset
        {
            get { return m_offset; }
            set
            {
                if (m_offset == value) return;
                m_offset = value;
                RaisePropertyChanged(() => this.Offset);
            }
        }

        Curve m_curve;
        public Curve Curve
        {
            get { return m_curve; }
            set {
                if (m_curve == value) return;
                m_curve = value;
                RaisePropertyChanged(() => this.Curve);
                RaisePropertyChanged(() => this.Label);
            }
        }

        Transform m_keyFrame;
        public Transform KeyFrame
        {
            get { return m_keyFrame; }
            set {
                //if (m_keyFrame == value) return;
                m_keyFrame = value;
                //RaisePropertyChanged(() => this.KeyFrame);
            }
        }

        public Transform LocalTransform
        {
            get
            {
                return new Transform(m_keyFrame.Translation + m_offset, m_keyFrame.Rotation);
            }
        }

        public Transform WorldTransform
        {
            get;
            set;
        }

        public void UpdateWorldTransform(Transform parent)
        {
            WorldTransform = LocalTransform * parent;

            foreach(var child in Children)
            {
                child.UpdateWorldTransform(WorldTransform);
            }
        }

        public Mesh Mesh
        {
            get;
            set;
        }

        #region Children
        ObservableCollection<Node> m_children;
        public ObservableCollection<Node> Children
        {
            get
            {
                if (m_children == null)
                {
                    m_children = new ObservableCollection<Node>();
                }
                return m_children;
            }
        }

        public IEnumerable<Tuple<Node, Node>> TraversePair()
        {
            foreach (var child in Children)
            {
                yield return Tuple.Create(this, child);

                foreach (var x in child.TraversePair())
                {
                    yield return x;
                }
            }
        }

        public IEnumerable<Node> Traverse()
        {
            yield return this;

            foreach (var child in Children)
            {
                foreach (var x in child.Traverse())
                {
                    yield return x;
                }
            }
        }

        public IEnumerable<T> Traverse<T>(Func<Node, SharpDX.Vector3, T> pred, SharpDX.Vector3 pos=new SharpDX.Vector3())
        {
            yield return pred(this, pos);

            foreach (var child in Children)
            {
                foreach(var x in child.Traverse(pred, pos+Offset))
                {
                    yield return x;
                }
            }
        }
        #endregion

        public void SetPose(Pose pose)
        {
            // キーフレームの更新
            foreach (var node in Traverse())
            {
                Transform value;
                if (!String.IsNullOrEmpty(node.Name)
                    && pose != null
                    && pose.Values.TryGetValue(node.Name, out value))
                {
                    node.KeyFrame = value;
                }
                else
                {
                    node.KeyFrame = Transform.Identity;
                }
            }

            // 積算
            UpdateWorldTransform(Transform.Identity);
        }

        public void UpdateVertexBuffer()
        {
            var mesh = Mesh;
            if (mesh == null) return;

            var gray = new SharpDX.Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            var white = new SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            var red = new SharpDX.Vector4(1.0f, 0, 0, 1.0f);

            var vertices = TraversePair()
                .Select(x =>
                {
                    if(x.Item2.IsSelected)
                    {
                        return new { line = x, color = red };
                    }
                    else if (x.Item2.KeyFrame == Transform.Identity)
                    {
                        return new { line = x, color = gray };
                    }
                    else
                    {
                        return new { line = x, color = white };
                    }
                })
                .SelectMany(x => new SharpDX.Vector4[] {
                    new SharpDX.Vector4(x.line.Item1.WorldTransform.Translation, 1.0f), x.color
                    , new SharpDX.Vector4(x.line.Item2.WorldTransform.Translation, 1.0f), x.color
                })
                .SelectMany(x => new Single[] { x.X, x.Y, x.Z, x.W })
                .ToArray()
                ;

            if (!vertices.Any()) return;

            // ToDO: Meshごとにシェーダーを見るべし
            // ToDo: 解放されている？
            var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * vertices.Length);
            Marshal.Copy(vertices, 0, ptr, vertices.Length);

            mesh.VertexBufferUpdate = VertexBufferUpdateCommand.Create(mesh.VertexBuffer, ptr);
        }
    }
}
