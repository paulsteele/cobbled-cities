using Minecraft.City.Datapack.Generator.Builder;
using Minecraft.City.Datapack.Generator.Models.PackMetadata;
using Minecraft.City.Datapack.Generator.Writers;
using Minecraft.City.Datapack.Generator.Writers.StaticWriters;

namespace Minecraft.City.Datapack.Generator;

public static class Program
{
	public static void Main()
	{
		var output = new DirectoryInfo("output");

		if (output.Exists)
		{
			output.Delete(true);
		}

		var dependencies = new Dependencies();
		
		var staticWriters = dependencies.GetService<IEnumerable<IStaticWriter>>();
		staticWriters.ToList().ForEach(w => w.Serialize());

		var packMetadata = dependencies.GetService<PackMetadata>();
		
		foreach (var assembler in dependencies.GetService<IEnumerable<IAssembler>>())
		{
			assembler.Assemble();
		}
		
		// var partBuildings = Dependencies.Container.Resolve<NbtPartAssembler>().AssembleBuildings(6);
		//
		// var start = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"start",
		// 	new[] { new TemplatePoolElementWeight("poke-cities:roads/s", 1) }
		// );
		// var twointer = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"twointer",
		// 	new[] { new TemplatePoolElementWeight("poke-cities:roads/2i", 1) }
		// );
		// var fiveinter = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"fiveinter",
		// 	new[] { new TemplatePoolElementWeight("poke-cities:roads/5i", 1) }
		// );
		// var tworoad = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"tworoad",
		// 	new[]
		// 	{
		// 		new TemplatePoolElementWeight("poke-cities:roads/2r", 10),
		// 		new TemplatePoolElementWeight("poke-cities:roads/p", 1)
		// 	}
		// );
		// var fiveroad = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"fiveroad",
		// 	new[]
		// 	{
		// 		new TemplatePoolElementWeight("poke-cities:roads/5r", 100),
		// 		new TemplatePoolElementWeight("poke-cities:roads/p", 5),
		// 		new TemplatePoolElementWeight("poke-cities:roads/2r", 1),
		// 	}
		// );
		// var park = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"park",
		// 	new[] { new TemplatePoolElementWeight("poke-cities:roads/p", 1) }
		// );
		//
		// var buildingTemplatePool = new TemplatePool
		// (
		// 	"data/poke-cities/worldgen/template_pool",
		// 	"buildings",
		// 	partBuildings.SelectMany(buildingBase =>
		// 	{
		// 		var buildingBaseArray = buildingBase as string[] ?? buildingBase.ToArray();
		// 		
		// 		var count = buildingBaseArray.Length;
		//
		// 		return buildingBaseArray.Select(single => new TemplatePoolElementWeight($"poke-cities:buildings/{single}", int.Max(150 / count, 1)));
		//
		// 	}).ToArray()
		// );
		//
		// var structure = new Structure
		// (
		// 	"data/poke-cities/worldgen/structure",
		// 	"poke-city",
		// 	start
		// );
		//
		// var cityStructure = new StructureSet
		// (
		// 	"data/poke-cities/worldgen/structure_set",
		// 	"poke-city",
		// 	50,
		// 	40,
		// 	new []{new StructureSetItem(structure, 1)}
		// );
		
		var jsonWriter = dependencies.GetService<JsonWriter>();
		
		jsonWriter.Serialize(packMetadata, ".mcmeta");
		// jsonWriter.Serialize(cityStructure);
		// jsonWriter.Serialize(structure);
		// jsonWriter.Serialize(start);
		// jsonWriter.Serialize(twointer);
		// jsonWriter.Serialize(tworoad);
		// jsonWriter.Serialize(fiveinter);
		// jsonWriter.Serialize(fiveroad);
		// jsonWriter.Serialize(park);
		// jsonWriter.Serialize(buildingTemplatePool);
		
		dependencies.GetService<JarWriter>().CreateJar();
	}
}