using KT_Interface.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class ImageCountViewModel : BindableBase
    {
        private int _total;
        public int Total
        {
            get
            {
                return _total;
            }
            set
            {
                SetProperty(ref _total, value);
            }
        }

        private int _ok;
        public int OK
        {
            get
            {
                return _ok;
            }
            set
            {
                SetProperty(ref _ok, value);
            }
        }

        private int _ng;
        public int NG
        {
            get
            {
                return _ng;
            }
            set
            {
                SetProperty(ref _ng, value);
            }
        }

        private int _skip;
        public int Skip
        {
            get
            {
                return _skip;
            }
            set
            {
                SetProperty(ref _skip, value);
            }
        }

        public DelegateCommand ClearCommand { get; set; }

        public ImageCountViewModel(InspectService inspectService)
        {
           inspectService.Inspected += Inspected;

           ClearCommand = new DelegateCommand(() =>
           {
               Total = OK = NG = Skip =0;
           });
        }

        private void Inspected(InspectResult result)
        {
            Total++;
            switch (result.Judgement)
            {
                case EJudgement.OK:
                    OK++;
                    break;
                case EJudgement.NG:
                    NG++;
                    break;
                case EJudgement.SKIP:
                    Skip++;
                    break;
            }
        }
    }
}