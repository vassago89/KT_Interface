using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Controls.ViewModels
{
    class ParameterControlViewModel : BindableBase
    {
        public ECameraParameter ParameterType { get; set; }

        private CameraParameter _cameraParameter;
        public CameraParameter CameraParameter
        {
            get
            {
                return _cameraParameter;
            }
            set
            {
                SetProperty(ref _cameraParameter, value);
                if (_cameraParameter != null)
                    Current = _cameraParameter.Current;
            }
        }

        private double _current;
        public double Current
        {
            get
            {
                return _current;
            }
            set
            {
                SetProperty(ref _current, value);
                _cameraParameter.Current = Math.Max(_cameraParameter.Min, Math.Min(_cameraParameter.Max, _current));
            }
        }

        public DelegateCommand SetCommand { get; }

        public ParameterControlViewModel(GrabService grabService)
        {
            SetCommand = new DelegateCommand(() =>
            {
                grabService.SetParameter(ParameterType, _cameraParameter.Current);
            });
        }
    }
}
