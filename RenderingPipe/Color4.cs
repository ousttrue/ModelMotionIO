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

        public static Color4 Green
        {
            get { return new Color4(0, 0.4f, 0, 1.0f); }
        }
    }
}
