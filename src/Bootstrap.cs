using Autofac;
using MonikTerminal.Interfaces;

namespace MonikTerminal
{
	public class Bootstrap
	{
		public static IContainer Container { get; private set; }

		public static IContainer Init()
		{
			var builder = new ContainerBuilder();

			//builder.RegisterType<OakApplication>().As<IOakApplication>().SingleInstance();
			//builder.RegisterType<Shell>().SingleInstance();
			builder.RegisterType<Config>().As<IConfig>().SingleInstance();
			builder.RegisterType<MonikService>().As<IMonikService>();
			builder.RegisterType<SourcesCache>().As<ISourcesCache>().SingleInstance();

			builder.RegisterType<LogTerminal>().As<ILogTerminal>().SingleInstance();
			builder.RegisterType<KeepAliveTerminal>().As<IKeepAliveTerminal>().SingleInstance();

			Container = builder.Build();
			
			return Container;
		}
	}
}