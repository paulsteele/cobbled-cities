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

		var roadPool = new TemplatePool
		(
			"data/poke-cities/worldgen/template_pool",
			"roads",
			new[] { new TemplatePoolElementWeight("poke-cities:roads/road_2", 1) }
		);
		
		var centerPool = new TemplatePool
		(
			"data/poke-cities/worldgen/template_pool",
			"center",
			new[] { new TemplatePoolElementWeight("poke-cities:roads/road_1", 1) }
		);

		var buildingTemplatePool = new TemplatePool
		(
			"data/poke-cities/worldgen/template_pool",
			"buildings",
			new[]
			{
				new TemplatePoolElementWeight("poke-cities:buildings/building_base_1", 1),
				new TemplatePoolElementWeight("poke-cities:buildings/building_base_2", 1),
			}
		);
		
		var structure = new Structure
		(
			"data/poke-cities/worldgen/structure",
			"poke-city",
			centerPool
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
		jsonWriter.Serialize(roadPool);
		jsonWriter.Serialize(centerPool);
		jsonWriter.Serialize(buildingTemplatePool);
		
		Dependencies.Container.Resolve<NbtStaticHandler>().CopyAndFixStaticFiles();
		var things = Dependencies.Container.Resolve<NbtPartAssembler>().AssembleBuildings(6);
		Dependencies.Container.Resolve<JarWriter>().CreateJar();
	}
}