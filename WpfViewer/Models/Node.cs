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

        public IEnumerable<T> Traverse<T>(Func<Node, SharpDX.Vector3, T> pred, SharpDX.Vector3 pos=new SharpDX.Vector3())
        {
            yield return pred(this, pos);

            foreach (var child in Children)
            {
                foreach(var x in child.Traverse(pred, pos+Position))
                {
                    yield return x;
                }
            }

        }
    }
}
