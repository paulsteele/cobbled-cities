namespace Minecraft.City.Datapack.Generator.Services;

public interface IBuildingZoneService
{
	IReadOnlyList<BuildingZone> Zones { get; }
	string GetPoolNameForRoadType(string roadTypeName);
	BuildingZone GetZoneForRoadType(string roadTypeName);
}

public class BuildingZoneService : IBuildingZoneService
{
	private readonly BuildingZone _centralZone = new("buildings-central", 6, 10);
	private readonly BuildingZone _urbanZone = new("buildings-urban", 4, 6);
	private readonly BuildingZone _residentialZone = new("buildings-residential", 3, 3);

	public IReadOnlyList<BuildingZone> Zones => new[] { _centralZone, _urbanZone, _residentialZone };

	public string GetPoolNameForRoadType(string roadTypeName)
	{
		return GetZoneForRoadType(roadTypeName).Name;
	}

	public BuildingZone GetZoneForRoadType(string roadTypeName)
	{
		return roadTypeName switch
		{
			"centers" => _centralZone,
			"cardinals" => _urbanZone,
			"inters" => _residentialZone,
			_ => throw new ArgumentException($"Unknown road type name: {roadTypeName}")
		};
	}
}