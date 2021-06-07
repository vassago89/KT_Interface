using KT_Interface.Core.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class ShellViewModel : BindableBase
    {
        StateStore _stateStore;
        public StateStore StateStore
        {
            get
            {
                return _stateStore;
            }
        }

        private InspectResult _result;
        public InspectResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                SetProperty(ref _result, value);
            }
        }

        public ShellViewModel(InspectService inspectService, StateStore stateStore)
        {
            _stateStore = stateStore;

            inspectService.Inspected += Inspected;
        }

        private void Inspected(InspectResult result)
        {
            Result = result;
            switch (result.Judgement)
            {
                case EJudgement.Pass:
                    break;
                case EJudgement.Fail:
                    break;
                case EJudgement.SKIP:
                    break;
            }
        }
    }
}