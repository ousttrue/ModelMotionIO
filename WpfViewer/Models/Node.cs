using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Models
{
    class Node : Livet.NotificationObject
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

    }
}
