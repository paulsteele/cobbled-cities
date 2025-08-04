using fNbt;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Services;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingAssembler(JsonWriter writer, IBuildingZoneService buildingZoneService) : IAssembler
{
	private const int MinHeight = 3;
	private const int MaxHeight = 10;

	public void Assemble()
	{
		var dynamicBuildings = DynamicBuilding.GetAllDynamicBuildings();

		var dynamicBuildingNames = dynamicBuildings.SelectMany(b => b.ConstructDynamicBuilding(MinHeight, MaxHeight)).ToArray();

		foreach (var zone in buildingZoneService.Zones)
		{
			var zoneBuildings = dynamicBuildingNames.Where(b => b.Height >= zone.MinHeight && b.Height <= zone.MaxHeight);
			var templatePool = CreateTemplatePool(zone.Name, zoneBuildings);
			writer.Serialize(templatePool);
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
