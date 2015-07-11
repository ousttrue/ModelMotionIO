using System;

namespace RenderingPipe
{
    public struct Color4
    {
        public Single R;
        public Single G;
        public Single B;
        public Single A;

        public Color4(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
