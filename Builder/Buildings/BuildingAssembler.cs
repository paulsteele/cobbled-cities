using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingAssembler(JsonWriter writer, IBuildingZoneService buildingZoneService) : IAssembler
{
	private const int MinHeight = 3;
	private const int MaxHeight = 20;

	public void Assemble()
	{
		var dynamicBuildings = DynamicBuilding.GetAllDynamicBuildings();

		var dynamicBuildingNames = dynamicBuildings.SelectMany(b => b.ConstructDynamicBuilding(MinHeight, MaxHeight)).ToArray();

		foreach (var zone in buildingZoneService.Zones)
		{
			foreach (var jigsawTileType in JigsawTileTypeExtensions.BuildingTypes)
			{
				var zoneBuildings = dynamicBuildingNames.Where(b => b.Height >= zone.MinHeight && b.Height <= zone.MaxHeight && b.JigsawTileType == jigsawTileType);
				var templatePool = CreateDynamicTemplatePool(zone.GetNameForType(jigsawTileType), zoneBuildings);
				writer.Serialize(templatePool);
			}
		}

		foreach (var extension in dynamicBuildingNames.Where(b => b.JigsawTileType == JigsawTileType.BuildingLongExtension))
		{
			var templatePool = CreateExtensionTemplatePool(extension);
			writer.Serialize(templatePool);
		}
	}

	private static TemplatePool CreateDynamicTemplatePool(string fileName, IEnumerable<BuildingInfo> dynamicBuildings)
	{
		return new TemplatePool(
			"data/cobbled-cities/worldgen/template_pool",
			$"{fileName}",
			dynamicBuildings.Select(building => 
				new TemplatePoolElementWeight($"cobbled-cities:buildings/{building.Name}", 1)
			).ToArray()
		);
	}
	
	private static TemplatePool CreateExtensionTemplatePool(BuildingInfo extension)
	{
		return new TemplatePool(
			"data/cobbled-cities/worldgen/template_pool",
			$"{extension.Name}",
			[
				new TemplatePoolElementWeight($"cobbled-cities:buildings/{extension.Name}", 1)
			]
		);
	}
}
