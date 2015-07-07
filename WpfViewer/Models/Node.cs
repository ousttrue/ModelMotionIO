using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Models
{
    class Node: Livet.NotificationObject
    {
        String m_name;
        public String Name
        {
            get { return m_name; }
            set {
                if (m_name == value) return;
                m_name = value;
                RaisePropertyChanged(() => this.Name);
            }
        }
    }
}
