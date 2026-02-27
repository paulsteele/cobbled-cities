using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingAssembler(JsonWriter writer, IBuildingZoneService buildingZoneService) : IAssembler
{
	private const string BuildingsBasePath = "../../../nbts/buildings";

	public void Assemble()
	{
		var universalPath = Path.Combine(BuildingsBasePath, buildingZoneService.UniversalFolderName);

		var globalMin = buildingZoneService.Zones.Min(z => z.MinHeight);
		var globalMax = buildingZoneService.Zones.Max(z => z.MaxHeight);

		var universalDynamicInfos = DynamicBuilding.GetAllDynamicBuildings(universalPath)
			.SelectMany(b => b.ConstructDynamicBuilding(globalMin, globalMax))
			.ToArray();

		var universalStaticInfos = StaticBuilding.GetAllStaticBuildings(universalPath).ToArray();

		var allExtensions = new List<BuildingInfo>();

		foreach (var zone in buildingZoneService.Zones)
		{
			var zonePath = Path.Combine(BuildingsBasePath, zone.FolderName);

			var zoneDynamicInfos = DynamicBuilding.GetAllDynamicBuildings(zonePath)
				.SelectMany(b => b.ConstructDynamicBuilding(zone.MinHeight, zone.MaxHeight))
				.ToArray();

			var zoneStaticInfos = StaticBuilding.GetAllStaticBuildings(zonePath).ToArray();

			var filteredUniversalDynamic = universalDynamicInfos
				.Where(b => b.Height >= zone.MinHeight && b.Height <= zone.MaxHeight);

			var allBuildingInfos = zoneDynamicInfos
				.Concat(filteredUniversalDynamic)
				.Concat(zoneStaticInfos)
				.Concat(universalStaticInfos)
				.ToArray();

			foreach (var jigsawTileType in JigsawTileTypeExtensions.BuildingTypes)
			{
				var buildings = allBuildingInfos
					.Where(b => b.JigsawTileType == jigsawTileType)
					.ToArray();

				var templatePool = CreateBalancedTemplatePool(zone.GetNameForType(jigsawTileType), buildings);
				writer.Serialize(templatePool);
			}

			allExtensions.AddRange(allBuildingInfos.Where(b => b.JigsawTileType == JigsawTileType.BuildingLongExtension));
		}

		foreach (var extension in allExtensions)
		{
			var templatePool = CreateExtensionTemplatePool(extension);
			writer.Serialize(templatePool);
		}
	}

	private static TemplatePool CreateBalancedTemplatePool(string fileName, BuildingInfo[] buildings)
	{
		var groups = buildings.GroupBy(b => b.Source).ToArray();

		var lcm = groups.Select(g => g.Count()).Aggregate(1, Lcm);

		return new TemplatePool(
			"data/cobbled-cities/worldgen/template_pool",
			fileName,
			buildings.Select(building =>
			{
				var sourceCount = groups.First(g => g.Key == building.Source).Count();
				var weight = lcm / sourceCount;
				return new TemplatePoolElementWeight($"cobbled-cities:buildings/{building.Name}", weight);
			}).ToArray()
		);
	}

	private static TemplatePool CreateExtensionTemplatePool(BuildingInfo extension)
	{
		return new TemplatePool(
			"data/cobbled-cities/worldgen/template_pool",
			extension.Name,
			[
				new TemplatePoolElementWeight($"cobbled-cities:buildings/{extension.Name}", 1)
			]
		);
	}

	private static int Gcd(int a, int b)
	{
		while (b != 0)
		{
			(a, b) = (b, a % b);
		}
		return a;
	}

	private static int Lcm(int a, int b) => a / Gcd(a, b) * b;
}
