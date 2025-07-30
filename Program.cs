using Minecraft.City.Datapack.Generator.Builder;
using Minecraft.City.Datapack.Generator.Models.PackMetadata;
using Minecraft.City.Datapack.Generator.Writers;
using Minecraft.City.Datapack.Generator.Writers.StaticWriters;

namespace Minecraft.City.Datapack.Generator;

public static class Program
{
	private static readonly Dependencies Dependencies = new();

	public static void Main()
	{
		var output = new DirectoryInfo("output");

		if (output.Exists)
		{
			output.Delete(true);
		}

		Dependencies.GetService<IEnumerable<IStaticWriter>>().ForEach(sw => sw.Serialize());

		Dependencies.GetService<IEnumerable<IAssembler>>().ForEach(a => a.Assemble());
		
		Dependencies.GetService<JsonWriter>().Serialize(Dependencies.GetService<PackMetadata>(), ".mcmeta");
		
		Dependencies.GetService<JarWriter>().CreateJar();
	}
}