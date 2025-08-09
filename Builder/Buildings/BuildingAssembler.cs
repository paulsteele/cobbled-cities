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
				var templatePool = CreateTemplatePool(zone.GetNameForType(jigsawTileType), zoneBuildings);
				writer.Serialize(templatePool);
			}
		}
	}

	private TemplatePool CreateTemplatePool(string fileName, IEnumerable<BuildingInfo> dynamicBuildings)
	{
		return new TemplatePool(
			"data/poke-cities/worldgen/template_pool",
			$"{fileName}",
			dynamicBuildings.Select(building => 
				new TemplatePoolElementWeight($"poke-cities:buildings/{building.Name}", 1)
			).ToArray()
		);
	}
}
