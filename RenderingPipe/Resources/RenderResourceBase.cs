using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources
{
    public abstract class RenderResourceBase: IRenderResource
    {
        public abstract RenderResourceType RenderResourceType { get; }

        public UInt32 ID
        {
            get;
            private set;
        }
        static volatile UInt32 UniqueID = 1;
        public RenderResourceBase()
        {
            ID = UniqueID++;
        }

        Int32 m_age;
        public Int32 Age
        {
            get { return m_age; }
        }
    }
}
