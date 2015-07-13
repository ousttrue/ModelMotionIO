using NLog;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using WpfViewer.Models;

namespace WpfViewer.ViewModels
{
    public class AnimationViewModel: Livet.NotificationObject
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
                    m_motions.Add(new Motion("none", 30));
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
                    CurrentTime
                        .Select(x => ActiveMotion.Value != null ? ActiveMotion.Value.GetPose(x) : null)
                        .Subscribe(x => CurrentPose.Value = x)
                        ;
                }
                return m_activeMotion;
            }
        }

        ReactiveProperty<Pose> m_currentPose;
        public ReactiveProperty<Pose> CurrentPose
        {
            get
            {
                if (m_currentPose == null)
                {
                    m_currentPose = new ReactiveProperty<Pose>();
                }
                return m_currentPose;
            }
        }

        #region Time
        Stopwatch m_stopwatch;
        public Stopwatch Stopwatch
        {
            get
            {
                if (m_stopwatch == null)
                {
                    m_stopwatch = new Stopwatch();
                }
                return m_stopwatch;
            }
        }

        ReactiveProperty<TimeSpan> m_currentTime;
        public ReactiveProperty<TimeSpan> CurrentTime
        {
            get
            {
                if (m_currentTime == null)
                {
                    m_currentTime = new ReactiveProperty<TimeSpan>();
                }
                return m_currentTime;
            }
        }

        Livet.Commands.ViewModelCommand m_rewindCommand;
        public ICommand RewindCommand
        {
            get
            {
                if (m_rewindCommand == null) {
                    m_rewindCommand = new Livet.Commands.ViewModelCommand(Rewind);
                }
                return m_rewindCommand;
            }
        }
        public void Rewind()
        {
            Stop();
            Stopwatch.Reset();
            CurrentTime.Value = TimeSpan.Zero;
        }

        Livet.Commands.ViewModelCommand m_startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (m_startCommand == null)
                {
                    m_startCommand = new Livet.Commands.ViewModelCommand(Start);
                }
                return m_startCommand;
            }
        }
        IDisposable m_timerSubscription;
        public void Start()
        {
            if (m_timerSubscription != null) return;

            Stopwatch.Start();
            m_timerSubscription=Observable.Interval(TimeSpan.FromMilliseconds(33))
                .Select(_ => Stopwatch.Elapsed)
                .Subscribe(x => CurrentTime.Value = x)
                ;
        }

        Livet.Commands.ViewModelCommand m_stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (m_stopCommand == null)
                {
                    m_stopCommand = new Livet.Commands.ViewModelCommand(Stop);                        
                }
                return m_stopCommand;
            }
        }
        public void Stop()
        {
            if (m_timerSubscription == null) return;
            m_timerSubscription.Dispose();
            m_timerSubscription = null;
            Stopwatch.Stop();
        }
        #endregion

        public AnimationViewModel()
        {
            ActiveMotion
                .Select(x => x==null ? "" : x.Name)
                .Subscribe(x => Logger.Info(x));
        }

        TimeSpan FrameToTimeSpan(int frame, int fps)
        {
            return TimeSpan.FromMilliseconds(frame * (1000 / fps));
        }

        Transform VmdBoneFrameToKeyFrame(MMIO.Mmd.VmdBoneFrame vmd, Single scale)
        {
            return new Transform(
                new SharpDX.Vector3(vmd.Position.X, vmd.Position.Y, vmd.Position.Z) * scale
                , new SharpDX.Quaternion(vmd.Rotation.X, vmd.Rotation.Y, vmd.Rotation.Z, vmd.Rotation.W));
        }

        public void LoadVmd(Uri uri, Single scale=1.58f/20.0f)
        {
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var vmd = MMIO.Mmd.VmdParse.Execute(bytes);

            var motion = new Motion(Path.GetFileName(uri.LocalPath), 30);
            motion.AddRange(
                vmd.BoneFrames
                .ToLookup(x => x.BoneName)
                .Select(x => new Curve(x.Key, x.ToDictionary(
                    y => y.Frame
                    , y => VmdBoneFrameToKeyFrame(y, scale)
                    ))));                

            motion.LastFrame = FrameToTimeSpan(vmd.BoneFrames.Max(x => x.Frame), 30);
            Motions.Add(motion);
            Logger.Info("Loaded: {0}", uri);
        }

        static float ToRadians(float degree)
        {
            return (float)(Math.PI * degree / 180.0f);
        }

        static SharpDX.Quaternion inverseQ(SharpDX.Quaternion q, Axis axis)
        {
            var qaxis = q.Axis;
            var qangle = q.Angle;
            switch (axis)
            {
                case Axis.None: return q;
                case Axis.X: return SharpDX.Quaternion.RotationAxis(new SharpDX.Vector3(-qaxis.X, qaxis.Y, qaxis.Z), -qangle);
                case Axis.Y: return SharpDX.Quaternion.RotationAxis(new SharpDX.Vector3(qaxis.X, -qaxis.Y, qaxis.Z), -qangle);
                case Axis.Z: return SharpDX.Quaternion.RotationAxis(new SharpDX.Vector3(qaxis.X, qaxis.Y, -qaxis.Z), -qangle);
            }
            throw new ArgumentException();
        }

        public Transform ToTransform(IEnumerator<Single> it, IEnumerable<MMIO.Bvh.ChannelType> channels, Single scale, Axis axis)
        {
            var t = SharpDX.Vector3.Zero;
            var r = SharpDX.Matrix.Identity;
            var axisX = axis == Axis.X ? -1.0f : 1.0f;
            var axisY = axis == Axis.Y ? -1.0f : 1.0f;
            var axisZ = axis == Axis.Z ? -1.0f : 1.0f;

            /*
            var axisRX = axis == Axis.X ? -1.0f : 1.0f;
            var axisRY = axis == Axis.Y ? -1.0f : 1.0f;
            var axisRZ = axis == Axis.Z ? -1.0f : 1.0f;
            */

            foreach (var channel in channels)
            {
                it.MoveNext();
                switch (channel)
                {
                    case MMIO.Bvh.ChannelType.Xposition:
                        t.X = it.Current * scale * axisX;
                        break;

                    case MMIO.Bvh.ChannelType.Yposition:
                        t.Y = it.Current * scale * axisY;
                        break;

                    case MMIO.Bvh.ChannelType.Zposition:
                        t.Z = it.Current * scale * axisZ;
                        break;

                    case MMIO.Bvh.ChannelType.Xrotation:
                        //r = r * SharpDX.Matrix.RotationX(ToRadians(it.Current));
                        r = SharpDX.Matrix.RotationX(ToRadians(it.Current)) * r;
                        break;

                    case MMIO.Bvh.ChannelType.Yrotation:
                        //r = r * SharpDX.Matrix.RotationY(ToRadians(it.Current));
                        r = SharpDX.Matrix.RotationY(ToRadians(it.Current)) * r;
                        break;

                    case MMIO.Bvh.ChannelType.Zrotation:
                        //r = r * SharpDX.Matrix.RotationZ(ToRadians(it.Current));
                        r = SharpDX.Matrix.RotationZ(ToRadians(it.Current)) * r;
                        break;

                    default:
                        throw new ArgumentException();
                }
            }

            var q = SharpDX.Quaternion.RotationMatrix(r);
            return new Transform(t, inverseQ(q, axis));
            //return new Transform(t, q);
        }

        public void LoadBvh(Uri uri, Single scale = 0.01f)
        {
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, true);

            var motion = new Motion(Path.GetFileName(uri.LocalPath), bvh.Fps);
            foreach(var frame in bvh.Frames)
            {
                var it = ((IEnumerable<Single>)frame).GetEnumerator();
                var pose = new Pose();
                pose.Values = new Dictionary<string, Transform>();
                foreach (var node in bvh.Root.Traverse((node, level) => node).Where(x => x.Name!="EndSite"))
                {
                    pose.Values[node.Name] = ToTransform(it, node.Channels, scale, Axis.Z);
                }
                if (it.MoveNext())
                {
                    throw new ArgumentException();
                }
                motion.AddPose(pose);
            }
            motion.LastFrame = FrameToTimeSpan(bvh.Frames.Length, bvh.Fps);
            Motions.Add(motion);
            Logger.Info("Loaded: {0}", uri);
        }
    }
}
