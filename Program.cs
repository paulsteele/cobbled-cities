using Autofac;
using Minecraft.City.Datapack.Generator.Content.PackMetadata;
using Minecraft.City.Datapack.Generator.Writers;
using Minecraft.City.Datapack.Generator.Writers.StaticWriters;

namespace Minecraft.City.Datapack.Generator;

public static class Program
{
	public static void Main()
	{
		var staticWriters = Dependencies.Container.Resolve<IEnumerable<IStaticWriter>>();
		staticWriters.ToList().ForEach(w => w.Serialize());

		var packMetadata = Dependencies.Container.Resolve<PackMetadata>();
		
		var jsonWriter = Dependencies.Container.Resolve<JsonWriter>();
		
		jsonWriter.Serialize(packMetadata);
		Dependencies.Container.Resolve<JarWriter>().CreateJar();
	}
}