using RenderingPipe.Commands;
using RenderingPipe.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace WpfViewer.Win32.D3D11
{
    public abstract class ShaderResourceSlot
    {
        public String Name { get; set; }
        public ShaderStage Stage { get; set; }
        public Int32 Index { get; set; }

        protected ShaderResourceSlot(String name, ShaderStage stage, int index)
        {
            Name = name;
            Stage = stage;
            Index = index;
        }
    }

    public class ShaderConstantBufferSlot : ShaderResourceSlot, IDisposable
    {
        public ShaderConstantBufferSlot(String name, ShaderStage stage, int slot) : base(name, stage, slot) { }

        public SharpDX.Direct3D11.Buffer Buffer { get; set; }
        public ConstantBuffeDictionary BackingStore { get; set; }

        #region IDisposable
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
                if (Buffer != null)
                {
                    Buffer.Dispose();
                    Buffer = null;
                }
                if (BackingStore != null)
                {
                    //BackingStore.Dispose();
                    BackingStore = null;
                }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
        #endregion
    }

    public class ShaderSRVSlot: ShaderResourceSlot
    {
        public ShaderSRVSlot(String name, ShaderStage stage, int slot) : base(name, stage, slot) { }
    }

    public class ShaderSamplerSlot : ShaderResourceSlot
    {
        public ShaderSamplerSlot(String name, ShaderStage stage, int slot) : base(name, stage, slot) { }
    }

    public abstract class BaseShaderStage : ResourceItemBase
    {
        public abstract ShaderStage ShaderStage { get; }
        public ShaderConstantBufferSlot[] ConstantBufferSlots { get; set; }
        public List<ShaderResourceSlot> SRVSlots { get; private set; }
        public List<ShaderResourceSlot> SamplerSlots { get; private set; }

        protected BaseShaderStage()
        {
            SRVSlots = new List<ShaderResourceSlot>();
            SamplerSlots = new List<ShaderResourceSlot>();
        }

        public bool SetConstantVariable(ShaderVariableSetCommand command)
        {
            foreach(var slot in ConstantBufferSlots)
            {
                if(slot.BackingStore.Set(command.Key, command.Value)){
                    return true;
                }
            }
            return false;
        }

        public ShaderResourceSlot GetSRVSlot(String key)
        {
            foreach (var slot in SRVSlots)
            {
                if (slot.Name == key)
                {
                    return slot;
                }
            }
            return null;
        }

        public void SetSampler(String key)
        {
            //m_psBuffer.Samplers = m_resources.SamplerManager.Get(command.Samplers);
        }
        
        #region IDisposalbe
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            base.Dispose();

            if (disposing)
            {
                // Free any other managed objects here.
                ConstantBufferSlots.ForEach(cb => cb.Dispose());
            }
        }
        #endregion
    }

    public class VertexShaderStage: BaseShaderStage
    {
        public override ShaderStage ShaderStage
        {
            get { return ShaderStage.Vertex; }
        }
        public SharpDX.Direct3D11.VertexShader Shader { get; set; }
        public SharpDX.Direct3D11.InputLayout VertexLayout { get; set; }

        #region IDisposalbe
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Shader != null)
                {
                    Shader.Dispose();
                    Shader = null;
                }
                if (VertexLayout != null)
                {
                    VertexLayout.Dispose();
                    VertexLayout = null;
                }
            }
        }
        #endregion
    }

    public class GeometryShaderStage: BaseShaderStage
    {
        public override ShaderStage ShaderStage
        {
            get { return ShaderStage.Geometry; }
        }
        public SharpDX.Direct3D11.GeometryShader Shader { get; set; }

        #region IDisposalbe
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Shader != null)
                {
                    Shader.Dispose();
                    Shader = null;
                }
            }
        }
        #endregion
    }

    public class PixelShaderStage: BaseShaderStage
    {
        public override ShaderStage ShaderStage
        {
            get { return ShaderStage.Pixel; }
        }
        public SharpDX.Direct3D11.PixelShader Shader { get; set; }

        #region IDisposalbe
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Shader != null)
                {
                    Shader.Dispose();
                    Shader = null;
                }
            }
        }
        #endregion
    }

    public enum Components
    {
        X = 1,
        XY = 2,
        XYZ = 3,
        XYZW = 4,
    }

    public enum VertexSemantic
    {
        POSITION,
        NORMAL,
        COLOR,
        TEXCOORD,
    }

    public class VertexInput
    {
        public String SemanticName;
        public VertexSemantic Semantic;
        public Type ComponentType;
        public Components Components;

        public VertexInput(String semanticName, Type componentType, Components components)
        {
            SemanticName = semanticName;
            Semantic = (VertexSemantic)Enum.Parse(typeof(VertexSemantic), semanticName);
            ComponentType = componentType;
            Components = components;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}_{2}", SemanticName, Components, ComponentType);
        }
    }

    public class VertexAttribute : Attribute
    {
        public String SemanticName { get; set; }
        public Int32 Components { get; set; }

        /// <summary>
        /// Non zero default value
        /// </summary>
        public Single[] Padding { get; set; }
    }

    public class ShaderManager : BaseResourceManager<BaseShaderStage>
    {
        static Tuple<FieldInfo, VertexAttribute> FindField(VertexInput input, IEnumerable<FieldInfo> fields)
        {
            foreach (var f in fields)
            {
                foreach (var a in f.GetCustomAttributes(true))
                {
                    var va = a as VertexAttribute;
                    if (va != null)
                    {
                        if (va.SemanticName == input.SemanticName)
                        {
                            if (va.Components > (int)input.Components)
                            {
                                throw new ArgumentException();
                            }
                            return Tuple.Create(f, va);
                        }
                    }
                }
            }

            return null;
        }

        public class VertexWriter<VERTEX>
        {
            public delegate void StreamWriter(SharpDX.DataStream w, VERTEX v);

            public StreamWriter Writer
            {
                get;
                set;
            }

            public Int32 Stride
            {
                get;
                set;
            }

            public VertexWriter(StreamWriter writer, Int32 stride)
            {
                Writer = writer;
                Stride = stride;
            }

            public Byte[] ToBytes(IEnumerable<VERTEX> vertices)
            {
                Byte[] buffer;
                using (var w = new SharpDX.DataStream(Stride * vertices.Count(), true, true))
                {
                    vertices.ForEach(v => Writer(w, v));
                    w.Position = 0;
                    buffer = new Byte[w.Length];
                    w.ReadRange(buffer, 0, buffer.Length);
                }
                return buffer;
            }
        }

        public static VertexBufferResource Create<T>(IEnumerable<T> vertices
            , VertexWriter<T> vertexWriter
            , IEnumerable<Int32> indices = null
            , IEnumerable<VertexBufferResource.SubMesh> submeshes = null)
        {
            if (submeshes == null)
            {
                if (indices == null)
                {
                    submeshes = new[] { new VertexBufferResource.SubMesh(vertices.Count()) };
                }
                else
                {
                    submeshes = new[] { new VertexBufferResource.SubMesh(indices.Count()) };
                }
            }

            return new VertexBufferResource
            {
                Vertices = vertexWriter.ToBytes(vertices),
                Stride = vertexWriter.Stride,
                Indices = indices != null ? indices.ToArray() : null,
                SubMeshes = submeshes.ToArray(),
            };
        }

        static public VertexWriter<VERTEX> GetVertexWriter<VERTEX>(Byte[] bytes)
        {
            var fields = typeof(VERTEX).GetFields();
            var inputs = ShaderManager.ParseByteCode(bytes);
            int stride = 0;

            var fieldWriters=new List<Action<SharpDX.DataStream, VERTEX>>();
            foreach(var input in inputs)
            {
                var fieldWithAttribute = FindField(input, fields);                  
                if(fieldWithAttribute!=null){
                    var field=fieldWithAttribute.Item1;
                    var va=fieldWithAttribute.Item2;
                    fieldWriters.Add((w, v)=>{
                        var method = typeof(SharpDX.DataStream).GetMethods().First(
                            m => {
                                var parameters=m.GetParameters();
                                if(parameters.Length==0){
                                    return false;
                                }
                                return parameters[0].ParameterType == field.FieldType;
                            });
                        method.Invoke(w, new object[]{field.GetValue(v)});

                        // padding
                        var padding=(int)input.Components-va.Components;
                        for (int i = 4-padding; i < 4; ++i)
                        {
                            if (va.Padding == null)
                            {
                                w.Write(0f);
                            }
                            else
                            {
                                w.Write(va.Padding[i]);
                            }
                        }
                    });
                }
                else
                {
                    // no input
                    // padding ?
                    throw new ArgumentException();
                }

                stride+=4 * (int)input.Components;
            }

            var writerType=typeof(VertexWriter<>).MakeGenericType(new []{typeof(VERTEX)});

            var vertexWriter = Activator.CreateInstance(writerType, new Object[]{
                new VertexWriter<VERTEX>.StreamWriter((s, vertex)=>{
                    fieldWriters.ForEach(writer=>writer(s, vertex));
                })
                , stride
            });

            return vertexWriter as VertexWriter<VERTEX>;
        }

        public static List<VertexInput> ParseByteCode(Byte[] bytes)
        {
            var inputs = new List<VertexInput>();
            var reflection = new SharpDX.D3DCompiler.ShaderReflection(bytes);
            for (int i = 0; i < reflection.Description.InputParameters; ++i)
            {
                SharpDX.D3DCompiler.ShaderParameterDescription v = reflection.GetInputParameterDescription(i);
                var m=v.MinPrecision;
                var formatWithSize = GetFormat(v.UsageMask, v.ComponentType);

                Type componentType = null;
                switch(v.ComponentType)
                {
                    case SharpDX.D3DCompiler.RegisterComponentType.Float32:
                        componentType=typeof(float);
                        break;

                    case SharpDX.D3DCompiler.RegisterComponentType.SInt32:
                        componentType=typeof(int);
                        break;

                    case SharpDX.D3DCompiler.RegisterComponentType.UInt32:
                        componentType=typeof(uint);
                        break;

                    default:
                        throw new ArgumentException();
                }

                Components components;
                if (v.UsageMask.HasFlag(
                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX
                        | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY
                        | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ
                        | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentW))
                {
                    components = Components.XYZW;
                }
                else if (v.UsageMask.HasFlag(
                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX
                        | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY
                        | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ))
                {
                    components = Components.XYZ;
                }
                else if (v.UsageMask.HasFlag(
                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX
                        | SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY))
                {
                    components = Components.XY;
                }
                else if (v.UsageMask.HasFlag(
                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX))
                {
                    components = Components.X;
                }
                else
                {
                    throw new ArgumentException();
                }

                var input = new VertexInput(v.SemanticName, componentType, components);
                inputs.Add(input);
            }
            return inputs;
        }

        public bool Ensure(SharpDX.Direct3D11.Device device, ShaderResource resource)
        {
            switch (resource.ShaderStage)
            {
                case ShaderStage.Vertex:
                    return EnsureVertexShader(device, resource);

                case ShaderStage.Geometry:
                    return EnsureGeometryShader(device, resource);

                case ShaderStage.Pixel:
                    return EnsurePixelShader(device, resource);

                default:
                    throw new NotImplementedException();
            }
        }
       
        void ReflectionConstants(SharpDX.Direct3D11.Device device
            , BaseShaderStage shader, Byte[] bytes)
        {
            var reflection = new SharpDX.D3DCompiler.ShaderReflection(bytes);

            shader.ConstantBufferSlots = Enumerable.Range(0, reflection.Description.ConstantBuffers)
                .Select(i => 
                {
                    var b=reflection.GetConstantBuffer(i);
                    var source = new ConstantBuffeDictionary();
                    for (int j = 0; j < b.Description.VariableCount; ++j)
                    {
                        var field = b.GetVariable(j);
                        source.AddField(field.Description.Name, field.Description.Size);
                    }

                    var buffer = new SharpDX.Direct3D11.Buffer(device, b.Description.Size
                        , SharpDX.Direct3D11.ResourceUsage.Default
                        , SharpDX.Direct3D11.BindFlags.ConstantBuffer
                        , SharpDX.Direct3D11.CpuAccessFlags.None
                        , SharpDX.Direct3D11.ResourceOptionFlags.None
                        , 0);

                    return new ShaderConstantBufferSlot(b.Description.Name, shader.ShaderStage, i)
                    {
                        BackingStore = source,
                        Buffer = buffer,
                    };
                })
                .ToArray();
                ;

                for (var i = 0; i < reflection.Description.BoundResources; ++i)
                {
                    var desc = reflection.GetResourceBindingDescription(i);
                    switch(desc.Type)
                    {
                        case SharpDX.D3DCompiler.ShaderInputType.Sampler:
                            shader.SamplerSlots.Add(new ShaderSamplerSlot(
                                desc.Name, shader.ShaderStage, desc.BindPoint));
                            break;

                        case SharpDX.D3DCompiler.ShaderInputType.Texture:
                            shader.SRVSlots.Add(new ShaderSRVSlot(
                                desc.Name, shader.ShaderStage, desc.BindPoint));
                            break;

                        default:
                            break;
                    }
                }
        }

        void ReflectionInput(SharpDX.Direct3D11.Device device
            , VertexShaderStage vertexShader, Byte[] bytes)
        {
            var inputElements = new List<SharpDX.Direct3D11.InputElement>();
            int offset = 0;

            var reflection = new SharpDX.D3DCompiler.ShaderReflection(bytes);
            if (reflection.Description.InputParameters == 0)
            {
                return;
            }
            for (int i = 0; i < reflection.Description.InputParameters; ++i)
            {
                var v = reflection.GetInputParameterDescription(i);
                var formatWithSize = GetFormat(v.UsageMask, v.ComponentType);

                inputElements.Add(new SharpDX.Direct3D11.InputElement(
                    v.SemanticName, 0, formatWithSize.Item1, offset, 0
                    ));

                offset += formatWithSize.Item2;
            }

            var signature = SharpDX.D3DCompiler.ShaderSignature.GetInputSignature(bytes);
            var vertexLayout = new SharpDX.Direct3D11.InputLayout(device, signature
                    , inputElements.ToArray());

            vertexShader.VertexLayout = vertexLayout;
        }

        VertexShaderStage CreateVertexShader(SharpDX.Direct3D11.Device device
            , Byte[] bytes)
        {
            var compiled = new SharpDX.Direct3D11.VertexShader(device, bytes);
            var vertexShader = new VertexShaderStage
            {
                Shader = compiled,
            };

            ReflectionInput(device, vertexShader, bytes);
            ReflectionConstants(device, vertexShader, bytes);

            return vertexShader;
        }

        bool EnsureVertexShader(SharpDX.Direct3D11.Device device, ShaderResource r)
        {
            if (Get(r.ID)!=null)
            {
                return false;
            }

            var vertexShader = CreateVertexShader(device, r.ByteCode);
            if (vertexShader == null)
            {
                return false;
            }
            Add(r.ID, vertexShader);

            return true;
        }
    
        GeometryShaderStage CreateGeometryShader(SharpDX.Direct3D11.Device device
            , SharpDX.D3DCompiler.CompilationResult bytes)
        {
            var compiled = new SharpDX.Direct3D11.GeometryShader(device, bytes);

            var geometryShader = new GeometryShaderStage
            {
                Shader = compiled,
            };

            ReflectionConstants(device, geometryShader, bytes);

            return geometryShader;
        }

        bool EnsureGeometryShader(SharpDX.Direct3D11.Device device, ShaderResource r)
        {
            if (Get(r.ID) != null)
            {
                return false;
            }

            var geometryShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.FromStream(new MemoryStream(r.ByteCode));
            var geometryShader = CreateGeometryShader(device, new SharpDX.D3DCompiler.CompilationResult(geometryShaderByteCode, new SharpDX.Result(), "OK"));
            if (geometryShader == null)
            {
                return false;
            }

            Add(r.ID, geometryShader);

            return true;
        }

        PixelShaderStage CreatePixelShader(SharpDX.Direct3D11.Device device
            , SharpDX.D3DCompiler.CompilationResult bytes)
        {
            var compiled = new SharpDX.Direct3D11.PixelShader(device, bytes);
            var pixelShader = new PixelShaderStage
            {
                Shader = compiled,
            };

            ReflectionConstants(device, pixelShader, bytes);

            return pixelShader;
        }

        bool EnsurePixelShader(SharpDX.Direct3D11.Device device, ShaderResource r)
        {
            if (Get(r.ID) != null)
            {
                return false;
            }

            var pixelShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.FromStream(new MemoryStream(r.ByteCode));
            var pixelShader = CreatePixelShader(device, new SharpDX.D3DCompiler.CompilationResult(pixelShaderByteCode, new SharpDX.Result(), "OK"));
            if (pixelShader == null)
            {
                return false;
            }

            Add(r.ID, pixelShader);

            return true;
        }
    }
}
