using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfViewer.ViewModels
{
    public class LogLevelToColorConverter : IValueConverter
    {
        public String Target
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = value as LogLevel;
            if (level == null) return "";

            if (level == LogLevel.Error)
            {
                return Target == "Background" ? "Red" : "White";
            }
            else if (level == LogLevel.Info)
            {
                return Target == "Background" ? "White" : "Black";
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
