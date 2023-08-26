using System.Reflection;
using Autofac;

namespace Minecraft.City.Datapack.Generator;

public static class Dependencies
{
	private static IContainer? _container;
	public static IContainer Container => _container ??= CreateContainer();

	private static IContainer CreateContainer()
	{
		var containerBuilder = new ContainerBuilder();

		ScanForImplementations(containerBuilder);
		RegisterManualImplementation(containerBuilder);

		return containerBuilder.Build();
	}

	private static void RegisterManualImplementation(ContainerBuilder builder)
	{
	}

	private static void ScanForImplementations(ContainerBuilder builder)
	{
		var currentAssembly = Assembly.GetExecutingAssembly();
		builder
			.RegisterAssemblyTypes(currentAssembly)
			.AsSelf()
			.AsImplementedInterfaces();
	}
}