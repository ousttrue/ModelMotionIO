using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Bullet
{
    public class Joint
    {
        public String Name { get; set; }

        public Int32 RigidBodyIndexA { get; set; }
        public Int32 RigidBodyIndexB { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 EulerAngleRadians { get; set; }

        /// <summary>
        /// 移動制限
        /// </summary>
        public Vector3 LinearLowerLimit { get; set; }

        /// <summary>
        /// 移動制限
        /// </summary>
        public Vector3 LinearUpperLimit { get; set; }

        /// <summary>
        /// 回転制限
        /// </summary>
        public Vector3 AngularLowerLimit { get; set; }

        /// <summary>
        /// 回転制限
        /// </summary>
        public Vector3 AngularUpperLimit { get; set; }

        /// <summary>
        /// 剛性
        /// </summary>
        public Vector3 LinearStiffness { get; set; }

        /// <summary>
        /// 剛性
        /// </summary>
        public Vector3 AngularStiffness { get; set; }
    }
}
