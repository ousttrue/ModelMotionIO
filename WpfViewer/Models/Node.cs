using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WpfViewer.Models
{
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
    }
}
