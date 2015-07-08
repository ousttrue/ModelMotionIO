using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Win32.D3D11
{
    public class ResourceItemBase: IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }

    public class BaseResourceManager<T> : IDisposable
        where T : ResourceItemBase
    {
        Dictionary<UInt32, T> m_resourceMap = new Dictionary<uint, T>();

        public T Get(UInt32 id)
        {
            lock (((ICollection)m_resourceMap).SyncRoot)
            {
                T resource;
                if (!m_resourceMap.TryGetValue(id, out resource))
                {
                    return null;
                }
                return resource;
            }
        }

        public List<T> Get(IEnumerable<UInt32> ids)
        {
            lock (((ICollection)m_resourceMap).SyncRoot)
            {
                var list = new List<T>();
                foreach (var id in ids)
                {
                    T resource;
                    if (!m_resourceMap.TryGetValue(id, out resource))
                    {
                        list.Add(null);
                    }
                    else
                    {
                        list.Add(resource);
                    }
                }
                return list;
            }
        }

        protected void Add(UInt32 id, T resource)
        {
            lock (((ICollection)m_resourceMap).SyncRoot)
            {
                m_resourceMap[id] = resource;
            }
        }

        static public Tuple<SharpDX.DXGI.Format, int> GetFormat(SharpDX.D3DCompiler.RegisterComponentMaskFlags usage
            , SharpDX.D3DCompiler.RegisterComponentType component)
        {
            if (usage.HasFlag(
                SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX
                    | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY
                    | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ
                    | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentW))
            {
                switch (component)
                {
                    case SharpDX.D3DCompiler.RegisterComponentType.Float32:
                        return Tuple.Create(SharpDX.DXGI.Format.R32G32B32A32_Float, 16);
                }
            }
            else if (usage.HasFlag(
                SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX
                    | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY
                    | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ))
            {
                switch (component)
                {
                    case SharpDX.D3DCompiler.RegisterComponentType.Float32:
                        return Tuple.Create(SharpDX.DXGI.Format.R32G32B32_Float, 12);
                }
            }
            else if (usage.HasFlag(
                SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX
                    | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY))
            {
                switch (component)
                {
                    case SharpDX.D3DCompiler.RegisterComponentType.Float32:
                        return Tuple.Create(SharpDX.DXGI.Format.R32G32_Float, 8);
                }
               
            }
            else if (usage.HasFlag(
                SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX))
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        #region IDisosable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                m_resourceMap.ForEach(kv =>
                {
                    kv.Value.Dispose();
                });
                m_resourceMap.Clear();
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
        #endregion
    }
}
