using Autofac;
using Minecraft.City.Datapack.Generator.StaticWriters;

namespace Minecraft.City.Datapack.Generator;

public static class Program
{
	public static void Main()
	{
		var staticWriters = Dependencies.Container.Resolve<IEnumerable<IStaticWriter>>();
		staticWriters.ToList().ForEach(w => w.Serialize());
		
		Dependencies.Container.Resolve<JarWriter>().CreateJar();
	}
}