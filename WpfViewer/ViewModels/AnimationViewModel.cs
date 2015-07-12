using NLog;
using Reactive.Bindings;
using System;
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

        Transform VmdBoneFrameToKeyFrame(MMIO.Mmd.VmdBoneFrame vmd)
        {
            return new Transform(
                new SharpDX.Vector3(vmd.Position.X, vmd.Position.Y, vmd.Position.Z)
                , new SharpDX.Quaternion(vmd.Rotation.X, vmd.Rotation.Y, vmd.Rotation.Z, vmd.Rotation.W));
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
                    y => y.Frame
                    , y => VmdBoneFrameToKeyFrame(y)))
                );

            motion.LastFrame = FrameToTimeSpan(vmd.BoneFrames.Max(x => x.Frame), 30);

            Motions.Add(motion);

            Logger.Info("Loaded: {0}", uri);
        }
    }
}
