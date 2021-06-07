using KT_Interface.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace KT_Interface.Converters
{
    class ResultColorConvereter : IValueConverter
    {
        Brush _okBrush;
        Brush _ngBrush;
        Brush _skipBrush;
        Brush _timeoutBrush;

        public ResultColorConvereter()
        {
            _okBrush = new SolidColorBrush(Colors.Green);
            _ngBrush = new SolidColorBrush(Colors.Red);
            _skipBrush = new SolidColorBrush(Colors.Yellow);
            _timeoutBrush = new SolidColorBrush(Colors.Blue);

            _okBrush.Freeze();
            _ngBrush.Freeze();
            _skipBrush.Freeze();
            _timeoutBrush.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as InspectResult;

            if (result == null)
                return null;

            switch (result.Judgement)
            {
                case EJudgement.Pass:
                    return _okBrush;
                case EJudgement.Fail:
                    return _ngBrush;
                case EJudgement.SKIP:
                    return _skipBrush;
                case EJudgement.TIMEOUT:
                    return _timeoutBrush;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
