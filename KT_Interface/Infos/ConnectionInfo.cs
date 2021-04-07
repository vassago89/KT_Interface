using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KT_Interface.Infos
{
    struct ConnectionInfo
    {
        bool _isConnected;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        string _message;
        public string Message
        {
            get
            {
                return _message;
            }
        }

        Brush _brush;
        public Brush Brush
        {
            get
            {
                return _brush;
            }
        }

        public ConnectionInfo(
            bool isConnected,
            string message,
            Color color)
        {
            _isConnected = isConnected;
            _message = message;
            _brush = new SolidColorBrush(color);
            _brush.Freeze();
        }
    }
}
