using NLog;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace WpfViewer.Models
{
    public struct KeyFrame
    {
        public SharpDX.Vector3 Translation;
        public SharpDX.Quaternion Rotation;
    }

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

    public class AnimationManager: Livet.NotificationObject
    {
        #region Logger
        static Logger Logger
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }
        #endregion

        ObservableCollection<Motion> m_motions;
        public ObservableCollection<Motion> Motions
        {
            get
            {
                if (m_motions == null) {
                    m_motions = new ObservableCollection<Motion>();
                    m_motions.Add(new Motion("none"));
                }
                return m_motions;
            }
        }

        ReactiveProperty<Motion> m_activeMotion;
        public ReactiveProperty<Motion> ActiveMotion
        {
            get {
                if (m_activeMotion == null)
                {
                    m_activeMotion = new ReactiveProperty<Motion>();
                }
                return m_activeMotion;
            }
        }

        public AnimationManager()
        {
            ActiveMotion
                .Select(x => x==null ? "" : x.Name)
                .Subscribe(x => Logger.Info(x));
        }

        TimeSpan FrameToTimeSpan(int frame, int frameParSecond)
        {
            return TimeSpan.FromMilliseconds(frame * 1000 / frameParSecond);
        }

        KeyFrame VmdBoneFrameToKeyFrame(MMIO.Mmd.VmdBoneFrame vmd)
        {
            return new KeyFrame
            {
                Translation = new SharpDX.Vector3(vmd.Position.X, vmd.Position.Y, vmd.Position.Z),
                Rotation = new SharpDX.Quaternion(vmd.Rotation.X, vmd.Rotation.Y, vmd.Rotation.Z, vmd.Rotation.W),
            };
        }

        public void LoadVmd(Uri uri)
        {
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var vmd = MMIO.Mmd.VmdParse.Execute(bytes);

            var motion = new Motion(Path.GetFileName(uri.LocalPath));
            motion.CurveMap = vmd.BoneFrames
                .ToLookup(x => x.BoneName)
                .ToDictionary(
                x => x.Key
                , x => new Curve(x.ToDictionary(
                    y => FrameToTimeSpan(y.Frame, 30)
                    , y => VmdBoneFrameToKeyFrame(y)))
                );

            Motions.Add(motion);

            Logger.Info("Loaded: {0}", uri);
        }
    }
}
