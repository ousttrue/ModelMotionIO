using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.ViewModels
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
