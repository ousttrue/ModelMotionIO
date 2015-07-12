using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Models
{
    public struct Transform
    {
        public readonly SharpDX.Vector3 Translation;
        public readonly SharpDX.Quaternion Rotation;

        public Transform(SharpDX.Vector3 t, SharpDX.Quaternion r)
        {
            Translation = t;
            Rotation = r;
        }

        // overload operator * 
        public static Transform operator *(Transform a, Transform b)
        {
            return new Transform(
                SharpDX.Vector3.Transform(a.Translation, b.Rotation) + b.Translation
                , a.Rotation * b.Rotation
                );
        }

        public static Transform Lerp(Transform a, Transform b, float c)
        {
            return new Transform(
                SharpDX.Vector3.Lerp(a.Translation, b.Translation, c)
                , SharpDX.Quaternion.Slerp(a.Rotation, b.Rotation, c)
                );
        }

        public static readonly Transform Identity = new Transform
        (SharpDX.Vector3.Zero, SharpDX.Quaternion.Identity);
    }

    public static class SortedListExtensions
    {
        public static Tuple<int, int> BinarySearch<S, T>(this SortedList<S, T> slist, S value)
        {
            var list = slist.Keys;
            var comp = Comparer<S>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                int m = (hi + lo) / 2;  // this might overflow; be careful.
                var c = comp.Compare(list[m], value);
                if (c < 0) lo = m + 1;
                else if (c > 0) hi = m - 1;
                else return Tuple.Create(m, m);
            }
            {
                var c = comp.Compare(list[lo], value);
                if (c < 0) return Tuple.Create(lo, lo+1);
                else if (c > 0) return Tuple.Create(lo-1, lo);
                else return Tuple.Create(lo, lo);
            }
        }
    }

    /// <summary>
    /// 一つの部位の連続した姿勢
    /// </summary>
    public class Curve
    {
        public int Fps
        {
            get;
            set;
        }

        public SortedList<int, Transform> Values
        {
            get;
            private set;
        }

        public Curve(IDictionary<int, Transform> values, int fps = 30)
        {
            Fps = fps;
            Values = new SortedList<int, Transform>(values);
        }

        double TimeToFrame(TimeSpan time)
        {
            return time.TotalSeconds * Fps;
        }

        /// <summary>
        /// 補間された値を得る
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Transform GetValue(TimeSpan time)
        {
            var frame = TimeToFrame(time);
            var range = Values.BinarySearch((int)frame);
            if (range.Item1 < 0)
            {
                // 開始フレーム
                return Values[Values.Keys[range.Item2]];
            }
            else if (range.Item2 >= Values.Count)
            {
                // 最終フレーム
                return Values[Values.Keys[range.Item1]];
            }
            else
            {
                if (range.Item1 == range.Item2 && range.Item1+1 >= Values.Count)
                {
                    // 最終フレーム
                    return Values[Values.Keys[range.Item1]];
                }
                var lower = Values.Keys[range.Item1];
                var upper = Values.Keys[range.Item1 + 1];

                // 補間する
                var rate = (frame - lower) / (upper - lower);
                return Transform.Lerp(Values[lower], Values[upper], (float)rate);
            }
        }
    }

    /// <summary>
    /// 一つの時間の各部位の姿勢
    /// </summary>
    public class Pose
    {
        public Dictionary<String, Transform> Values
        {
            get;
            set;
        }
    }

    public class Motion
    {
        public String Name { get; set; }
        public TimeSpan LastFrame { get; set; }

        public String Label
        {
            get
            {
                return String.Format("{0}({1:00}分{2:00})", Name, LastFrame.Hours * 60 + LastFrame.Minutes, LastFrame.Seconds);
            }
        }

        public Motion(String name)
        {
            Name = name;
        }

        public Dictionary<String, Curve> CurveMap
        {
            get;
            set;
        }

        public Pose GetPose(TimeSpan time)
        {
            return new Pose { Values = CurveMap.ToDictionary(x => x.Key, x => x.Value.GetValue(time)) };
        }
    }
}
