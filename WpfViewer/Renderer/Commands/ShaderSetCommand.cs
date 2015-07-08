using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Renderer.Commands
{
    public class ShaderSetCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.Shader_Set; }
        }

        public override string ToString()
        {
            return String.Format("Set {0}", ShaderStage);
        }

        public ShaderStage ShaderStage { get; set; }

        public UInt32 ResourceID
        {
            get;
            private set;
        }

        public static ShaderSetCommand Create(ShaderResource resource)
        {
            if (resource == null)
            {
                return null;
            }
            return new ShaderSetCommand
            {
                ShaderStage = resource.ShaderStage,
                ResourceID = resource.ID,
            };
        }
    }
}
