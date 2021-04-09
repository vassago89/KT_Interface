using KT_Interface.Core;
using NLog;
using NLog.Targets;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using KT_Interface.Core.Services;
using System.IO;

namespace KT_Interface.ViewModels
{
    class LogViewModel : BindableBase
    {
        private ObservableCollection<LogEventInfo> _logs;
        public IEnumerable<LogEventInfo> Logs
        {
            get
            {
                return _logs;
            }
        }

        public DelegateCommand LogFolderOpenCommand { get; private set; }
        public StateStore StateStore { get; private set; }

        private CoreConfig _coreConfig;
        
        public LogViewModel(StateStore stateStore, CoreConfig coreConfig)
        {
            _logs = new ObservableCollection<LogEventInfo>();
            BindingOperations.EnableCollectionSynchronization(_logs, new object());

            _coreConfig = coreConfig;

            StateStore = stateStore;

            LogFolderOpenCommand = new DelegateCommand(() =>
            {
                if (Directory.Exists(_coreConfig.LogPath))
                {
                    Process.Start(Path.GetFullPath(_coreConfig.LogPath));
                }   
            });
        }

        public void Recived(LogEventInfo logEvent)
        {
            if (_logs.Count > 0  && _logs.Count >= _coreConfig.MaxLogCount)
                _logs.RemoveAt(0);
            
            _logs.Insert(0, logEvent);
        }
    }
}
