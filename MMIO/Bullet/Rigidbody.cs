using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Bullet
{
    public enum RigidbodyShapeType
    {
        Sphere,
        Box,
        Capsule,
    }

    public enum RigidbodyOperationType
    {
        Kinematics,
        Dynamics,
        DynamicsRotation,
    }

    public class Rigidbody
    {
        public String Name { get; set; }
        public String EnglishName { get; set; }
        public Int32? BoneIndex { get; set; }

        /// <summary>
        /// 衝突グループ
        /// </summary>
        public Byte CollisionGroup { get; set; }

        /// <summary>
        /// 衝突無視グループ
        /// </summary>
        public UInt16 CollisionIgnoreGroup { get; set; }

        /// <summary>
        /// 衝突形状タイプ
        /// </summary>
        public RigidbodyShapeType ShapeType { get; set; }

        /// <summary>
        /// 衝突形状サイズ
        /// </summary>
        public Vector3 ShapeSize { get; set; }

        /// <summary>
        /// 初期姿勢(位置)
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// 初期姿勢(回転)
        /// </summary>
        public Vector3 EulerAngleRadians { get; set; }

        /// <summary>
        /// 質量
        /// </summary>
        public Single Mass { get; set; }

        /// <summary>
        /// 移動減衰
        /// </summary>
        public Single LinearDamping { get; set; }

        /// <summary>
        /// 回転減衰
        /// </summary>
        public Single AngularDamping { get; set; }

        /// <summary>
        /// 反発係数
        /// </summary>
        public Single Restitution { get; set; }

        /// <summary>
        /// 摩擦
        /// </summary>
        public Single Friction { get; set; }

        /// <summary>
        /// 処理タイプ
        /// </summary>
        public RigidbodyOperationType OperationType { get; set; }
    }
}
