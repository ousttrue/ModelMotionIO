using SharpDX;
using System;

namespace WpfViewer.Models
{
    /// <summary>
    /// 姿勢
    /// </summary>
    public class TransformationBase
    {
        Matrix m_matrix = Matrix.Identity;
        public Matrix Matrix
        {
            get { return m_matrix; }
            set
            {
                if (m_matrix == value)
                {
                    return;
                }
                m_matrix = value;
                EmitMatrixChanged();
            }
        }

        public event EventHandler MatrixChanged;
        void EmitMatrixChanged()
        {
            var tmp = MatrixChanged;
            if (tmp != null)
            {
                tmp(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// 移動・回転・拡大縮小
    /// </summary>
    public class Transformation : TransformationBase
    {
        void CalcMatrix()
        {
            Matrix = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Scaling, Vector3.Zero, Rotation, Translation);
        }

        Vector3 m_translation = Vector3.Zero;
        public Vector3 Translation
        {
            get { return m_translation; }
            set
            {
                if (m_translation == value)
                {
                    return;
                }
                m_translation = value;
                CalcMatrix();
            }
        }

        Quaternion m_rotation = Quaternion.Identity;
        public Quaternion Rotation
        {
            get { return m_rotation; }
            set
            {
                if (m_rotation == value)
                {
                    return;
                }
                m_rotation = value;
                CalcMatrix();
            }
        }

        Vector3 m_scaling = Vector3.One;
        public Vector3 Scaling
        {
            get { return m_scaling; }
            set
            {
                if (m_scaling == value)
                {
                    return;
                }
                m_scaling = value;
                CalcMatrix();
            }
        }
    }

    /// <summary>
    /// 注視点を中心に回転する
    /// </summary>
    public class OrbitTransformation : TransformationBase
    {
        public OrbitTransformation()
        {
            CalcView();
        }

        double m_shiftX = 0;
        double m_shiftY = 0;
        public void AddShift(double x, double y)
        {
            m_shiftX += x * m_distance;
            m_shiftY += y * m_distance;
            CalcView();
        }

        double m_distance = 5;
        public void Dolly(double d)
        {
            m_distance *= d;
            CalcView();
        }

        double m_yawRadians = 0;
        public void AddYaw(double rad)
        {
            m_yawRadians += rad;
            CalcView();
        }

        double m_pitchRadians = 0;
        public void AddPitch(double rad)
        {
            m_pitchRadians += rad;
            CalcView();
        }
        double m_rollRadians = 0;

        public void CalcView()
        {
            //Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix = Matrix.RotationY((float)m_yawRadians)
                * Matrix.RotationX((float)m_pitchRadians)
                * Matrix.Translation((float)m_shiftX, (float)m_shiftY, (float)m_distance)
                ;
        }
    }

    public class PerspectiveProjection
    {
        public PerspectiveProjection()
        {
            CalcProjection();
        }

        public Matrix Matrix
        {
            get;
            private set;
        }

        double m_zNear = 0.1;
        public double ZNear
        {
            get { return m_zNear; }
            set
            {
                m_zNear = value;
                CalcProjection();
            }
        }

        double m_zFar = 1000.0;
        public double ZFar
        {
            get { return m_zFar; }
            set
            {
                m_zFar = value;
                CalcProjection();
            }
        }

        double m_fovYRadians = Math.PI / 4.0;
        public double FovYRadians
        {
            get { return m_fovYRadians; }
            set
            {
                m_fovYRadians = value;
                CalcProjection();
            }
        }

        Vector2 m_size = new Vector2(1.0f, 1.0f);
        public Vector2 TargetSize
        {
            get { return m_size; }
            set
            {
                if (m_size == value)
                {
                    return;
                }
                m_size = value;
                CalcProjection();
            }
        }

        public void CalcProjection()
        {
            if (TargetSize.Y == 0)
            {
                Matrix = Matrix.Identity;
                return;
            }

            Matrix = Matrix.PerspectiveFovLH((float)FovYRadians
                , (float)TargetSize.X / (float)TargetSize.Y
                , (float)ZNear, (float)ZFar);
        }
    }
}
