using System;
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
    }
}
