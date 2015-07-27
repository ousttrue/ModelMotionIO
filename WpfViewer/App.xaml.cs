using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfViewer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(
                MMIO.ViewModels.ObservableMemoryTarget.Instance, LogLevel.Trace);
        }
    }
}
