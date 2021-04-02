using KT_Interface.Core;
using KT_Interface.ViewModels;
using KT_Interface.Views;
using Newtonsoft.Json;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace KT_Interface
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Register();

			var shell = new ShellView();
			shell.ShowDialog();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			var config = ContainerRegistry.Container.Resolve<AppConfig>();
			File.WriteAllText("AppConfig.json", JsonConvert.SerializeObject(config));
			
			base.OnExit(e);
		}

		private void Register()
		{
			ContainerRegistry.Container
				.RegisterInstance(
                File.Exists("AppConfig.json") 
                ? JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText("AppConfig.json"))
                : new AppConfig())
                .RegisterInstance(
                File.Exists("CoreConfig.json")
                ? JsonConvert.DeserializeObject<CoreConfig>(File.ReadAllText("CoreConfig.json"))
                : new CoreConfig())
                .RegisterType<ShellViewModel>()
				.RegisterType<SubViewModel>();
		}
	}
}
