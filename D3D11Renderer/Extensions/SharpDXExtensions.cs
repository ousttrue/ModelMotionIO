namespace D3D11.Extensions
{
    public static class SharpDXExtensions
    {
        public static SharpDX.Color4 ToSharpDX(this RenderingPipe.Color4 src)
        {
            return new SharpDX.Color4(src.R, src.G, src.B, src.A);
        }
    }
}
