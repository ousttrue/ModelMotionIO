using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Models
{
    public struct KeyFrame
    {
        public SharpDX.Vector3 Translation;
        public SharpDX.Quaternion Rotation;
    }

    /// <summary>
    /// 一つの部位の連続した姿勢
    /// </summary>
    public class Curve
    {
        public SortedList<TimeSpan, KeyFrame> Values
        {
            get;
            private set;
        }

        public Curve(IDictionary<TimeSpan, KeyFrame> values)
        {
            Values = new SortedList<TimeSpan, KeyFrame>(values);
        }
    }

    /// <summary>
    /// 一つの時間の各部位の姿勢
    /// </summary>
    public class Pose
    {
        public Dictionary<String, KeyFrame> Values
        {
            get;
            set;
        }
    }

    public class Motion
    {
        public String Name { get; set; }

        public Motion(String name)
        {
            Name = name;
        }

        public Dictionary<String, Curve> CurveMap
        {
            get;
            set;
        }
    }
}
