using fNbt;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingAssembler(JsonWriter writer) : IAssembler
{
	private const int MinHeight = 3;
	private const int MaxHeight = 10;

	public void Assemble()
	{
		var dynamicBuildings = DynamicBuilding.GetAllDynamicBuildings();

		var dynamicBuildingNames = dynamicBuildings.SelectMany(b => b.ConstructDynamicBuilding(MinHeight, MaxHeight)).ToArray();

		var central = dynamicBuildingNames.Where(b => b.Height >= 6);
		var urban = dynamicBuildingNames.Where(b => b.Height is >= 4 and <= 6);
		var residential = dynamicBuildingNames.Where(b => b.Height == 3);

		var centralPool = CreateTemplatePool("buildings-central", central);
		var urbanPool = CreateTemplatePool("buildings-urban", urban);
		var residentialPool = CreateTemplatePool("buildings-residential", residential);

		writer.Serialize(centralPool);
		writer.Serialize(urbanPool);
		writer.Serialize(residentialPool);
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
