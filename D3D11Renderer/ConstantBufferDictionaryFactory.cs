using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


namespace D3D11
{
    public class AllocatedPtr : IDisposable
    {
        public IntPtr Ptr { get; private set; }
        public Int32 Size { get; private set; }

        public AllocatedPtr(int size)
        {
            Ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            Size = size;
        }

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
            }

            // Free any unmanaged objects here.
            if (Ptr != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }

            disposed = true;
        }

        ~AllocatedPtr()
        {
            Dispose(false);
        }

        public static AllocatedPtr From(byte[] buffer)
        {
            var ptr = new AllocatedPtr(buffer.Length);
            Marshal.Copy(buffer, 0, ptr.Ptr, buffer.Length);
            return ptr;
        }

        public static AllocatedPtr From(params Object[] args)
        {
            var size = args.Select(arg => Marshal.SizeOf(arg.GetType())).Sum();
            var ptr = new AllocatedPtr(size);
            int offset = 0;
            foreach (var arg in args)
            {
                // copy
                Marshal.StructureToPtr(arg, ptr.Ptr + offset, false);

                offset += Marshal.SizeOf(arg.GetType());
            }

            return ptr;
        }
    }

    public class ConstantBuffeDictionary
    {
        public class Field
        {
            public String Key;
            public Int32 Size;
            public Int32 Offset;
        }
        public List<Field> Fields
        {
            get;
            private set;
        }

        public AllocatedPtr Buffer
        {
            get;
            private set;
        }

        public Int32 Size
        {
            get
            {
                return Fields.Select(f => f.Size).Sum();
            }
        }

        public ConstantBuffeDictionary()
        {
            Fields= new List<Field>();
        }
 
        public void AddField(String key, Int32 size)
        {
            Fields.Add(new Field
            {
                Key = key,
                Size = size,
                Offset = Size,
            });
        }

        public bool Set(String key, Object value)
        {
            foreach (var f in Fields)
            {
                if (f.Key == key)
                {
                    if (Buffer == null)
                    {
                        Buffer = new AllocatedPtr(Size);
                    }

                    // copy value
                    Marshal.StructureToPtr(value, Buffer.Ptr + f.Offset, false);

                    return true;
                }
            }
            return false;
        }
    }

    public class ConstantBufferDictionaryFactory
    {
        public static ConstantBuffeDictionary From(SharpDX.D3DCompiler.ConstantBuffer c)
        {
            var cb=new ConstantBuffeDictionary();

            for (int i = 0; i < c.Description.VariableCount; ++i)
            {
                var v = c.GetVariable(i);
                cb.AddField(v.Description.Name, v.Description.Size);    
            }

            return cb;
        }

        public static IEnumerable<ConstantBuffeDictionary> From(SharpDX.D3DCompiler.ShaderBytecode byteCode)
        {
            var reflection = new SharpDX.D3DCompiler.ShaderReflection(byteCode);
            return Enumerable.Range(0, reflection.Description.ConstantBuffers)
            .Select(i => reflection.GetConstantBuffer(i))
            .Select(c => ConstantBufferDictionaryFactory.From(c))
            ;
        }
    }
}
