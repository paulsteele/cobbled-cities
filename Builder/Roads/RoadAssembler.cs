using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Buildings;
using Minecraft.City.Datapack.Generator.Models.IlNodes;
using Minecraft.City.Datapack.Generator.Models.Structure;
using Minecraft.City.Datapack.Generator.Models.StructureSet;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

// ReSharper disable once ClassNeverInstantiated.Global
public class RoadAssembler(JsonWriter writer, IBuildingZoneService buildingZoneService) : IAssembler
{
	public void Assemble()
	{
		var centers = new RoadZone(new DirectoryInfo("../../../nbts/centers"), "centers", new IlPoint(85, 85)); // 170, 170
		var cardinals = new RoadZone(new DirectoryInfo("../../../nbts/cardinals"), "cardinals", new IlPoint(85,  170 + 94)); // 170, 187
		var inters = new RoadZone(new DirectoryInfo("../../../nbts/inters"), "inters", new IlPoint(-85, 136 + 85)); // 136, 136

		centers.NextZone = cardinals;
		cardinals.NextZone = inters;

		var centerStartingSections = AssembleType(centers);
		var cardinalStartingSections = AssembleType(cardinals);
		var interStartingSections = AssembleType(inters);
		
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
		
		writer.Serialize(startingPool);
		writer.Serialize(cardinalsPool);
		writer.Serialize(intersPool);
		writer.Serialize(structure);
		writer.Serialize(cityStructure);
	}

	private List<(string name, RoadSection section)> AssembleType(RoadZone zone)
	{
		var files = zone.GetFiles();

		var startingSections = new List<(string, RoadSection)>();

		foreach (var file in files.Where(f => f.Extension == ".nbt"))
		{
			DeconstructFile(file, zone, startingSections);
		}

		return startingSections;
	}

	private void DeconstructFile(
		FileSystemInfo fileInfo, 
		RoadZone zone,
		List<(string, RoadSection)> startingSections
	)
	{
		var nbt = new NbtFile(fileInfo.FullName);

		var road = new RoadSection(nbt.RootTag);
		
		road.DebugPrint();
		
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
			subSection.UpdateJigsaws(fileName, subSectionDictionary, zone, buildingZoneService);
			subSection.FillEmptySpace();
			subSection.SaveNbt(fileName, zone.Name);
			
			subSection.DebugPrint();
			
			if (subSection.IsCenter())
			{
				startingSections.Add((fileName, subSection));
				continue;
			}
			
			var templatePool = subSection.CreateTemplatePool(fileName, zone.Name);
			
			writer.Serialize(templatePool);
		}
	}
}
