using Autofac;
using Minecraft.City.Datapack.Generator.Content.PackMetadata;
using Minecraft.City.Datapack.Generator.Content.Structure;
using Minecraft.City.Datapack.Generator.Content.StructureSet;
using Minecraft.City.Datapack.Generator.Content.TemplatePool;
using Minecraft.City.Datapack.Generator.NbtHandler;
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

		var templatePool = new TemplatePool
		(
			"data/poke-cities/worldgen/template_pool",
			"roads",
			new[] { new TemplatePoolElementWeight("poke-cities:roads/road_1", 1) }
		);

		var buildingTemplatePool = new TemplatePool
		(
			"data/poke-cities/worldgen/template_pool",
			"buildings",
			new[] { new TemplatePoolElementWeight("poke-cities:buildings/building_1", 1) }
		);
		
		var structure = new Structure
		(
			"data/poke-cities/worldgen/structure",
			"poke-city",
			templatePool
		);
		
		var cityStructure = new StructureSet
		(
			"data/poke-cities/worldgen/structure_set",
			"poke-city",
			20,
			10,
			new []{new StructureSetItem(structure, 1)}
		);
		
		var jsonWriter = Dependencies.Container.Resolve<JsonWriter>();
		
		jsonWriter.Serialize(packMetadata, ".mcmeta");
		jsonWriter.Serialize(cityStructure);
		jsonWriter.Serialize(structure);
		jsonWriter.Serialize(templatePool);
		jsonWriter.Serialize(buildingTemplatePool);
		
		Dependencies.Container.Resolve<NbtFileFixer>().CopyAndFixFiles();
		Dependencies.Container.Resolve<JarWriter>().CreateJar();
	}
}