using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingAssembler(JsonWriter writer, IBuildingZoneService buildingZoneService) : IAssembler
{
	private const string BuildingsBasePath = "../../../nbts/buildings";
	private const string PoolBasePath = "data/cobbled-cities/worldgen/template_pool";

	public void Assemble()
	{
		var universalPath = Path.Combine(BuildingsBasePath, buildingZoneService.UniversalFolderName);

		var globalMin = buildingZoneService.Zones.Min(z => z.MinHeight);
		var globalMax = buildingZoneService.Zones.Max(z => z.MaxHeight);

		// Generate all floor sections once — universal buildings use global height range
		var universalFloorInfos = DynamicBuilding.GetAllDynamicBuildings(universalPath)
			.SelectMany(b => b.ConstructFloorSections(globalMin, globalMax))
			.ToArray();

		var universalStaticInfos = StaticBuilding.GetAllStaticBuildings(universalPath).ToArray();

		// Generate zone-specific floor sections once per zone
		var zoneFloorInfosByZone = new Dictionary<BuildingZone, FloorSectionInfo[]>();
		var zoneStaticInfosByZone = new Dictionary<BuildingZone, BuildingInfo[]>();

		foreach (var zone in buildingZoneService.Zones)
		{
			var zonePath = Path.Combine(BuildingsBasePath, zone.FolderName);

			zoneFloorInfosByZone[zone] = DynamicBuilding.GetAllDynamicBuildings(zonePath)
				.SelectMany(b => b.ConstructFloorSections(zone.MinHeight, zone.MaxHeight))
				.ToArray();

			zoneStaticInfosByZone[zone] = StaticBuilding.GetAllStaticBuildings(zonePath).ToArray();
		}

		// Create zone-type building pools (containing bottoms + statics)
		foreach (var zone in buildingZoneService.Zones)
		{
			var zoneFloorInfos = zoneFloorInfosByZone[zone];
			var zoneStaticInfos = zoneStaticInfosByZone[zone];

			var filteredUniversalBottoms = universalFloorInfos
				.Where(f => f is { FloorType: FloorType.Bottom, IsExtension: false }
					&& f.Height >= zone.MinHeight && f.Height <= zone.MaxHeight);

			var zoneBottoms = zoneFloorInfos
				.Where(f => f is { FloorType: FloorType.Bottom, IsExtension: false });

			foreach (var jigsawTileType in JigsawTileTypeExtensions.BuildingTypes)
			{
				var bottoms = zoneBottoms
					.Concat(filteredUniversalBottoms)
					.Where(f => f.JigsawTileType == jigsawTileType)
					.ToArray();

				var statics = zoneStaticInfos
					.Concat(universalStaticInfos)
					.Where(b => b.JigsawTileType == jigsawTileType)
					.Select(b => new FloorSectionInfo
					{
						Name = b.Name,
						Source = b.Source,
						FloorType = FloorType.Bottom,
						Height = null,
						ChainStep = null,
						JigsawTileType = b.JigsawTileType,
						IsExtension = false
					})
					.ToArray();

				var allEntries = bottoms.Concat(statics).ToArray();

				if (allEntries.Length == 0)
				{
					continue;
				}

				var templatePool = CreateBalancedTemplatePool(
					zone.GetNameForType(jigsawTileType),
					allEntries);
				writer.Serialize(templatePool);
			}

			// Static building extension pools
			var staticExtensions = zoneStaticInfos
				.Concat(universalStaticInfos)
				.Where(b => b.JigsawTileType == JigsawTileType.BuildingLongExtension);

			foreach (var extension in staticExtensions)
			{
				var templatePool = CreateSingleEntryPool(extension.Name, extension.Name);
				writer.Serialize(templatePool);
			}
		}

		// Collect all floor infos for mid/top/extension pool generation
		var allFloorInfos = universalFloorInfos
			.Concat(zoneFloorInfosByZone.Values.SelectMany(x => x))
			.ToArray();

		// Generate mid chain step pools and top pools per building source
		var mainFloorsBySource = allFloorInfos
			.Where(f => !f.IsExtension)
			.GroupBy(f => f.Source);

		foreach (var sourceGroup in mainFloorsBySource)
		{
			var source = sourceGroup.Key;

			// Mid step pools: floors-mid-{source}-step-{N}
			var midsByStep = sourceGroup
				.Where(f => f.FloorType == FloorType.Mid)
				.GroupBy(f => f.ChainStep!.Value);

			foreach (var stepGroup in midsByStep)
			{
				var step = stepGroup.Key;
				var poolName = $"floors-mid-{source}-step-{step}";
				var elements = stepGroup
					.Select(f => new TemplatePoolElementWeight($"cobbled-cities:buildings/{f.Name}", 1))
					.ToArray();

				writer.Serialize(new TemplatePool(PoolBasePath, poolName, elements));
			}

			// Top pool: floors-top-{source}
			var tops = sourceGroup
				.Where(f => f.FloorType == FloorType.Top)
				.ToArray();

			if (tops.Length > 0)
			{
				var topPoolName = $"floors-top-{source}";
				var topElements = tops
					.Select(f => new TemplatePoolElementWeight($"cobbled-cities:buildings/{f.Name}", 1))
					.ToArray();

				writer.Serialize(new TemplatePool(PoolBasePath, topPoolName, topElements));
			}
		}

		// Extension pools for long building floor sections
		foreach (var extension in allFloorInfos.Where(f => f.IsExtension))
		{
			var templatePool = CreateSingleEntryPool(extension.Name, extension.Name);
			writer.Serialize(templatePool);
		}
	}

	private static TemplatePool CreateBalancedTemplatePool(string fileName, FloorSectionInfo[] sections)
	{
		var groups = sections.GroupBy(b => b.Source).ToArray();
		var lcm = groups.Select(g => g.Count()).Aggregate(1, Lcm);

		return new TemplatePool(
			PoolBasePath,
			fileName,
			sections.Select(section =>
			{
				var sourceCount = groups.First(g => g.Key == section.Source).Count();
				var weight = lcm / sourceCount;
				return new TemplatePoolElementWeight($"cobbled-cities:buildings/{section.Name}", weight);
			}).ToArray()
		);
	}

	private static TemplatePool CreateSingleEntryPool(string poolName, string structureName)
	{
		return new TemplatePool(
			PoolBasePath,
			poolName,
			[new TemplatePoolElementWeight($"cobbled-cities:buildings/{structureName}", 1)]
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
