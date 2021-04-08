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

namespace KT_Interface
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : PrismApplication
	{
        private CancellationTokenSource _cts;

        protected override void OnExit(ExitEventArgs e)
		{
            File.WriteAllText("CoreConfig.json", JsonConvert.SerializeObject(Container.Resolve<CoreConfig>()));

            base.OnExit(e);
		}

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _cts = new CancellationTokenSource();

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
                .RegisterSingleton<LightControlService>();

            var config = Container.Resolve<CoreConfig>();
            if (config.CameraInfo != null)
                Container.Resolve<GrabService>().Connect(config.CameraInfo);
                
            if (config.LightSerialInfo != null)
                Container.Resolve<LightControlService>().Connect();

            Container.Resolve<HostCommService>().Connect();

            Container.Resolve<StoringService>().Run();
        }

        protected override Window CreateShell()
        {
            return new ShellView();
        }
    }
}
