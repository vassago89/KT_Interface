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

        public ResultColorConvereter()
        {
            _okBrush = new SolidColorBrush(Colors.Green);
            _ngBrush = new SolidColorBrush(Colors.Red);
            _skipBrush = new SolidColorBrush(Colors.Yellow);

            _okBrush.Freeze();
            _ngBrush.Freeze();
            _skipBrush.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as InspectResult;

            if (result == null)
                return null;

            switch (result.Judgement)
            {
                case EJudgement.OK:
                    return _okBrush;
                case EJudgement.NG:
                    return _ngBrush;
                case EJudgement.SKIP:
                    return _skipBrush;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
