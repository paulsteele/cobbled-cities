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

		var centers = dynamicBuildingNames.Where(b => b.Height >= 6);
		var cardinals = dynamicBuildingNames.Where(b => b.Height is >= 4 and <= 6);
		var inters = dynamicBuildingNames.Where(b => b.Height == 3);

		var centerPool = CreateTemplatePool("buildings-centers", centers);
		var cardPool = CreateTemplatePool("buildings-cardinals", cardinals);
		var interPool = CreateTemplatePool("buildings-inters", inters);

		writer.Serialize(centerPool);
		writer.Serialize(cardPool);
		writer.Serialize(interPool);
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