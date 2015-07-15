using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reactive.Linq;

namespace SharpDXScene
{
    public class Node
    {
        ReactiveProperty<String> m_name;
        public ReactiveProperty<String> Name
        {
            get {
                if (m_name == null)
                {
                    m_name = new ReactiveProperty<string>();
                }
                return m_name;
            }
        }

        public Node(String name)
        {
            Name.Value = name;
        }

        public Node(String name, SharpDX.Vector3 position)
        {
            Name.Value = name;
            Position.Value = position;
        }

        public Node(String name, SharpDX.Vector3 position, SharpDX.Vector3 offset)
        {
            Name.Value = name;
            Position.Value = position;
            Offset.Value = offset;
        }

        ReactiveProperty<bool> m_isSelected;
        public ReactiveProperty<Boolean> IsSelected
        {
            get {
                if (m_isSelected == null)
                {
                    m_isSelected = new ReactiveProperty<bool>();
                }
                return m_isSelected;
            }
        }

        ReactiveProperty<String> m_label;
        public ReactiveProperty<String> Label
        {
            get
            {
                if (m_label==null)
                {
                    m_label = Name
                      .ToReactiveProperty()
                      ;
                }
                return m_label;
            }
        }

        /// <summary>
        /// Model原点からの位置
        /// </summary>
        ReactiveProperty<SharpDX.Vector3> m_position;
        public ReactiveProperty<SharpDX.Vector3> Position
        {
            get {
                if (m_position == null)
                {
                    m_position = new ReactiveProperty<SharpDX.Vector3>();
                }
                return m_position;
            }
        }

        /// <summary>
        /// 親ボーンからの位置
        /// </summary>
        ReactiveProperty<SharpDX.Vector3> m_offset;
        public ReactiveProperty<SharpDX.Vector3> Offset
        {
            get {
                if (m_offset == null)
                {
                    m_offset = new ReactiveProperty<SharpDX.Vector3>();
                }
                return m_offset;
            }
        }

        ReactiveProperty<Transform> m_keyFrame;
        public ReactiveProperty<Transform> KeyFrame
        {
            get {
                if (m_keyFrame == null)
                {
                    m_keyFrame = new ReactiveProperty<Transform>(Transform.Identity);
                }
                return m_keyFrame;
            }
        }

        ReactiveProperty<Transform> m_localTransform;
        public ReactiveProperty<Transform> LocalTransform
        {
            get
            {
                if (m_localTransform == null)
                {
                    m_localTransform=
                    KeyFrame.CombineLatest(Offset, (keyFrame, offset) =>
                    {
                        return new Transform(keyFrame.Translation + offset, keyFrame.Rotation);
                    })
                    .ToReactiveProperty()
                    ;
                }
                return m_localTransform;
            }
        }

        public Transform WorldTransform
        {
            get
            {
                if (Parent == null) return LocalTransform.Value;
                return LocalTransform.Value * Parent.WorldTransform;
            }
        }

        #region Children
        public Node Parent
        {
            get;
            private set;
        }

        ObservableCollection<Node> m_childrenSrc= new ObservableCollection<Node>();
        ReadOnlyObservableCollection<Node> m_children;
        public ReadOnlyObservableCollection<Node> Children
        {
            get
            {
                if (m_children == null)
                {
                    m_children = new ReadOnlyObservableCollection<Node>(m_childrenSrc);
                }
                return m_children;
            }
        }
        public void AddChild(Node child)
        {
            child.Parent = this;
            m_childrenSrc.Add(child);
        }
        public void ClearChildren()
        {
            foreach (var child in Children)
            {
                child.Parent = null;
            }
            m_childrenSrc.Clear();
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
                foreach(var x in child.Traverse(pred, pos+Offset.Value))
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
                if (!String.IsNullOrEmpty(node.Name.Value)
                    && pose != null
                    && pose.Values.TryGetValue(node.Name.Value, out value))
                {
                    node.KeyFrame.Value = value;
                }
                else
                {
                    node.KeyFrame.Value = Transform.Identity;
                }
            }
        }
    }
}
