using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpDXScene
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
                , b.Rotation * a.Rotation
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

        #region Equals
        public override bool Equals(object obj)
        {
            return this.Equals((Transform)obj);
        }

        public bool Equals(Transform rhs)
        {
            // If parameter is null, return false. 
            if (Object.ReferenceEquals(rhs, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (Object.ReferenceEquals(this, rhs))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false. 
            if (this.GetType() != rhs.GetType())
                return false;

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return (Translation == rhs.Translation) && (Rotation == rhs.Rotation);
        }

        public override int GetHashCode()
        {
            return Translation.GetHashCode() * 0x00010000 + Rotation.GetHashCode();
        }

        public static bool operator ==(Transform lhs, Transform rhs)
        {
            // Check for null on left side. 
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true. 
                    return true;
                }

                // Only the left side is null. 
                return false;
            }
            // Equals handles case of null on right side. 
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Transform lhs, Transform rhs)
        {
            return !(lhs == rhs);
        }
        #endregion
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
        public String Name
        {
            get;
            private set;
        }

        public SortedList<int, Transform> Values
        {
            get;
            private set;
        }

        public Curve(String name)
        {
            Name = name;
            Values = new SortedList<int, Transform>();
        }

        public Curve(String name, IDictionary<int, Transform> values)
        {
            Name = name;
            Values = new SortedList<int, Transform>(values);
        }

        public override string ToString()
        {
            return String.Format("[{0}(1)]", Name, Values.Count);
        }

        /// <summary>
        /// 補間された値を得る
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Transform GetValue(double frame)
        {
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

    public class Motion: KeyedCollection<String, Curve>
    {
        protected override String GetKeyForItem(Curve item)
        {
            // In this example, the key is the part number.
            return item.Name;
        }

        public String Name { get; set; }
        public TimeSpan LastFrame { get; set; }

        public int Fps
        {
            get;
            private set;
        }

        double TimeToFrame(TimeSpan time)
        {
            return time.TotalSeconds * Fps;
        }

        public String Label
        {
            get
            {
                return String.Format("{0}({1:00}分{2:00})", Name, LastFrame.Hours * 60 + LastFrame.Minutes, LastFrame.Seconds);
            }
        }

        public Motion(String name, int fps) : base()
        {
            Name = name;
            Fps = fps;
        }

        public void AddRange(IEnumerable<Curve> curves)
        { 
            foreach (var curve in curves)
            {
                Add(curve);
            }
        }

        public void AddPose(Pose pose)
        {
            foreach (var kv in pose.Values)
            {
                Curve curve;
                if (!TryGetValue(kv.Key, out curve))
                {
                    curve = new Curve(kv.Key, new Dictionary<int, Transform>());
                    curve.Values.Add(0, kv.Value);
                    Add(curve);
                }
                else {
                    curve.Values.Add(curve.Values.Last().Key+1, kv.Value);
                }
            }
        }

        public Pose GetPose(TimeSpan time)
        {
            var frame = TimeToFrame(time);
            return new Pose { Values = this.ToDictionary(x => x.Name, x => x.GetValue(frame)) };
        }

        public bool TryGetValue(String key, out Curve curve)
        {
            /*
            try
            {
                curve=this[key];
                return true;
            }
            catch(KeyNotFoundException)
            {
                curve = null;
                return false;
            }
            */
            if (Contains(key))
            {
                curve=this[key];
                return true;
            }
            else
            {
                curve = null;
                return false;

            }
        }
    }
}
