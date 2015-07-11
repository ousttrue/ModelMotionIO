using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Extensions
{
    public static class SharpDXExtensions
    {
        public static SharpDX.Color4 ToSharpDX(this RenderingPipe.Color4 src)
        {
            return new SharpDX.Color4(src.R, src.G, src.B, src.A);
        }
    }
}
