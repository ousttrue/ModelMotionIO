using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Renderer.Commands
{
    public class ShaderTextureSetCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.ShaderTexture_Set; }
        }

        public String Key
        {
            get;
            private set;
        }


        public UInt32 ResourceID
        {
            get;
            private set;
        }

        public static ShaderTextureSetCommand Create(String key, TextureResource resource)
        {
            if (resource == null) return null;

            return new ShaderTextureSetCommand
            {
                Key = key,
                ResourceID = resource.ID,
            };
        }
    }
}
