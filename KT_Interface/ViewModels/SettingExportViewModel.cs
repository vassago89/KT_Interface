using KT_Interface.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class SettingExportViewModel : BindableBase
    {
        private string _resultPath;
        public string ResultPath
        {
            get
            {
                return _resultPath;
            }
            set
            {
                SetProperty(ref _resultPath, value);
            }
        }

        private string _logPath;
        public string LogPath
        {
            get
            {
                return _logPath;
            }
            set
            {
                SetProperty(ref _logPath, value);
            }
        }

        public CoreConfig CoreConfig { get; private set; }

        public DelegateCommand ResultPathCommand { get; private set; }
        public DelegateCommand LogPathCommand { get; private set; }

        public IEnumerable<ESaveMode> SaveModes { get; set; }
        public IEnumerable<ImageFormat> ImageFormats { get; set; }

        public SettingExportViewModel(CoreConfig coreConfig)
        {
            CoreConfig = coreConfig;

            SaveModes = Enum.GetValues(typeof(ESaveMode)).Cast<ESaveMode>();
            ImageFormats = new ImageFormat[] { ImageFormat.Bmp, ImageFormat.Png, ImageFormat.Jpeg };

            ResultPath = coreConfig.ResultPath;
            LogPath = coreConfig.LogPath;

            ResultPathCommand = new DelegateCommand(() =>
            {
                var dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = CoreConfig.ResultPath;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    CoreConfig.ResultPath = dialog.FileName;
                    ResultPath = CoreConfig.ResultPath;
                }
            });

            LogPathCommand = new DelegateCommand(() =>
            {
                var dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = CoreConfig.LogPath;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    CoreConfig.LogPath = dialog.FileName;
                    LogPath = CoreConfig.LogPath;
                }
            });
        }
    }
}
