using KT_Interface.Core;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KT_Interface.ViewModels
{
    class ObservableValue<T> : BindableBase
    {
        public Action<T> ValueChanged;

        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetProperty(ref _value, value);
                if (ValueChanged != null)
                    ValueChanged(_value);
            }
        }

        public ObservableValue(T value)
        {
            _value = value;
        }
    }

    class SettingViewModel : BindableBase
    {
        public DelegateCommand SaveCommand { get; set; }
        
        public SettingViewModel(CoreConfig coreConfig)
        {
            SaveCommand = new DelegateCommand(() =>
            {
                File.WriteAllText("CoreConfig.json", JsonConvert.SerializeObject(coreConfig));
            });
        }
    }
}
