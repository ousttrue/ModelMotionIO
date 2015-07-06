using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO
{
    public struct Vector2
    {
        public Single X;
        public Single Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Vector3
    {
        public Single X;
        public Single Y;
        public Single Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
