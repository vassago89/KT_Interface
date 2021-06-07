using KT_Interface.Core;
using KT_Interface.Core.Services;
using KT_Interface.ViewModels;
using KT_Interface.Views;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using NLog;
using System.Threading;
using System.Windows.Data;
using NLog.Targets;

namespace KT_Interface
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : PrismApplication
	{
        private CancellationTokenSource _cts;
        private ILogger _logger;

        protected override void OnExit(ExitEventArgs e)
		{
            File.WriteAllText("CoreConfig.json", JsonConvert.SerializeObject(Container.Resolve<CoreConfig>()));

            Container.Resolve<InspectService>().Disconnect();
            Container.Resolve<GrabService>().Disconnect();

            _logger.Info("Exit");
            base.OnExit(e);
		}

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            try
            {
                containerRegistry
                    .RegisterInstance(
                    File.Exists("CoreConfig.json")
                    ? JsonConvert.DeserializeObject<CoreConfig>(File.ReadAllText("CoreConfig.json"))
                    : new CoreConfig())
                    .RegisterInstance(_cts)
                    .Register<CancellationToken>(i => _cts.Token)
                    .RegisterSingleton<AppState>()
                    .RegisterSingleton<StateStore>()
                    .RegisterSingleton<GrabService>()
                    .RegisterSingleton<HostCommService>()
                    .RegisterSingleton<InspectService>()
                    .RegisterSingleton<LightControlService>()
                    .RegisterSingleton<StoringService>()
                    .RegisterSingleton<UsageService>()
                    .RegisterSingleton<LogService>()
                    .RegisterSingleton<LogViewModel>();

                var config = Container.Resolve<CoreConfig>();

                var logService = Container.Resolve<LogService>();
                var logViewModel = Container.Resolve<LogViewModel>();
                logService.Recived += logViewModel.Recived;

                var logConfig = new NLog.Config.LoggingConfiguration();
                var fileTarget = new FileTarget()
                {
                    Encoding = System.Text.Encoding.UTF8,
                    FileName = Path.Combine(config.LogPath, "KT_Interface.log"),
                    ArchiveFileName = Path.Combine(config.LogPath, "KT_Interface.{#}.log"),
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyy-MM-dd",
                    MaxArchiveDays = config.LogStoringDays,
                };
                
                logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logService);
                logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
                
                LogManager.Configuration = logConfig;

                _logger = LogManager.GetCurrentClassLogger();
                _logger.Info("Start");

                _cts = new CancellationTokenSource();
                
                

                Container.Resolve<HostCommService>().Connect();
                Container.Resolve<InspectService>().Connect();
                Container.Resolve<StoringService>().Run();

                if (config.CameraInfo != null)
                    Container.Resolve<GrabService>().Connect(config.CameraInfo);

                if (config.LightSerialInfo != null)
                    Container.Resolve<LightControlService>().Connect();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        protected override Window CreateShell()
        {
            try
            {
                return new ShellView();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return null;
            }
        }
    }
}
