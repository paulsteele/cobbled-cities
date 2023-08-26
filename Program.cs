using Autofac;
using Minecraft.City.Datapack.Generator.Content.PackMetadata;
using Minecraft.City.Datapack.Generator.Content.Structure;
using Minecraft.City.Datapack.Generator.Content.StructureSet;
using Minecraft.City.Datapack.Generator.Content.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;
using Minecraft.City.Datapack.Generator.Writers.StaticWriters;

namespace Minecraft.City.Datapack.Generator;

public static class Program
{
	public static void Main()
	{
		var output = new DirectoryInfo("output");
		
		output.Delete(true);
		
		var staticWriters = Dependencies.Container.Resolve<IEnumerable<IStaticWriter>>();
		staticWriters.ToList().ForEach(w => w.Serialize());

		var packMetadata = Dependencies.Container.Resolve<PackMetadata>();

		var structure = new Structure
		(
			"data/poke-cities/worldgen/structure",
			"city",
			"poke-cities:city"
		);
		var cityStructure = new StructureSet
		(
			"data/poke-cities/worldgen/structure_set",
			"city",
			4,
			2,
			new []{new StructureSetItem(structure, 1)}
		);

		var templatePool = new TemplatePool
		(
			"data/poke-cities/worldgen/template_pool",
			"city",
			"poke-cities:city",
			new[] { new TemplatePoolElementWeight("poke-cities:road", 1) }
		);
		
		var jsonWriter = Dependencies.Container.Resolve<JsonWriter>();
		
		jsonWriter.Serialize(packMetadata, ".mcmeta");
		jsonWriter.Serialize(cityStructure);
		jsonWriter.Serialize(structure);
		jsonWriter.Serialize(templatePool);
		
		var nbtCopyWriter = Dependencies.Container.Resolve<NbtCopyWriter>();
		nbtCopyWriter.Serialize();
		
		Dependencies.Container.Resolve<JarWriter>().CreateJar();
	}
}