using fNbt;
using Minecraft.City.Datapack.Generator.Models.Structure;
using Minecraft.City.Datapack.Generator.Models.StructureSet;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

// ReSharper disable once ClassNeverInstantiated.Global
public class RoadAssembler : IAssembler
{
	private readonly JsonWriter _writer;

	public RoadAssembler(JsonWriter writer)
	{
		_writer = writer;
	}
	
	public void Assemble()
	{
		var centers = new DirectoryInfo("../../../nbts/centers");
		var cardinals = new DirectoryInfo("../../../nbts/cardinals");
		var inters = new DirectoryInfo("../../../nbts/inters");
		
		var centerStartingSections = AssembleType(centers, nameof(centers), nameof(cardinals));
		var cardinalStartingSections = AssembleType(cardinals, nameof(cardinals), nameof(inters));
		var interStartingSections = AssembleType(inters, nameof(inters), "");
		
		var startingPool = new TemplatePool(
			"data/poke-cities/worldgen/template_pool",
			$"poke-cities",
			centerStartingSections.Select(
				s => new TemplatePoolElementWeight($"poke-cities:{nameof(centers)}/{s.name}-{s.section.Index}", 1)
			)
		);
		
		var cardinalsPool = new TemplatePool(
			"data/poke-cities/worldgen/template_pool",
			$"cardinals",
			cardinalStartingSections.Select(
				s => new TemplatePoolElementWeight($"poke-cities:{nameof(cardinals)}/{s.name}-{s.section.Index}", 1)
			)
		);
		
		var intersPool = new TemplatePool(
			"data/poke-cities/worldgen/template_pool",
			$"inters",
			interStartingSections.Select(
				s => new TemplatePoolElementWeight($"poke-cities:{nameof(inters)}/{s.name}-{s.section.Index}", 1)
			)
		);
		
		var structure = new Structure
		(
			"data/poke-cities/worldgen/structure",
			"poke-city",
			startingPool
		);
		
		var cityStructure = new StructureSet
		(
			"data/poke-cities/worldgen/structure_set",
			"poke-city",
			128,
			64,
			[new StructureSetItem(structure, 1)]
		);
		
		_writer.Serialize(startingPool);
		_writer.Serialize(cardinalsPool);
		_writer.Serialize(intersPool);
		_writer.Serialize(structure);
		_writer.Serialize(cityStructure);
	}

	private List<(string name, RoadSection section)> AssembleType(DirectoryInfo directory, string typeName, string outsideName)
	{
		var files = directory.GetFiles();

		var startingSections = new List<(string, RoadSection)>();

		foreach (var file in files.Where(f => f.Extension == ".nbt"))
		{
			DeconstructFile(file, typeName, startingSections, outsideName);
		}

		return startingSections;
	}

	private void DeconstructFile(
		FileSystemInfo fileInfo, 
		string typeName, 
		List<(string, RoadSection)> startingSections,
		string outsideName
	)
	{
		var nbt = new NbtFile(fileInfo.FullName);

		var road = new RoadSection(nbt.RootTag);
		
		var subSections = new List<RoadSection>();
		
		while (road.HasSubSections)
		{
			subSections.Add(road.TakeSubSection());
		}

		var subSectionDictionary = subSections
			.SelectMany(s => s.Jigsaws.Values.Select(k => (k.OriginalLocation, s.Index)))
			.ToDictionary();
		
		var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

		foreach (var subSection in subSections)
		{
			subSection.UpdateJigsaws(fileName, subSectionDictionary, typeName, outsideName);
			subSection.SaveNbt(fileName, typeName);
			
			if (subSection.IsCenter())
			{
				startingSections.Add((fileName, subSection));
				continue;
			}
			
			var templatePool = subSection.CreateTemplatePool(fileName, typeName);
			
			_writer.Serialize(templatePool);
		}
	}
}