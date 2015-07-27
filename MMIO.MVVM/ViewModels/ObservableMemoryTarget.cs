using NLog;
using NLog.Targets;
using System;
using System.Reactive.Subjects;

namespace MMIO.ViewModels
{
    [Target("MemoryTarget")]
    public class ObservableMemoryTarget : TargetWithLayout
    {
        Subject<LogEventInfo> m_logSubject = new Subject<LogEventInfo>();
        public IObservable<LogEventInfo> LogObservable
        {
            get { return m_logSubject; }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            m_logSubject.OnNext(logEvent);
        }

        #region Singleton
        ObservableMemoryTarget()
        { }

        public static ObservableMemoryTarget Instance = new ObservableMemoryTarget();
        #endregion
    }
}
