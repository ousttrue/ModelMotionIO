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

    public struct Vector4
    {
        public Single X;
        public Single Y;
        public Single Z;
        public Single W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }


    public struct Quaternion
    {
        public Single X;
        public Single Y;
        public Single Z;
        public Single W;

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
