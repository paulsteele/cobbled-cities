namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public interface IBuildingZoneService
{
	IReadOnlyList<BuildingZone> Zones { get; }
	BuildingZone GetZoneByDistance(double distance);
}

public class BuildingZoneService : IBuildingZoneService
{
	private readonly BuildingZone _centralZone = new("buildings-central", 20, 20, 4 * 16);
	private readonly BuildingZone _urbanZone = new("buildings-urban", 10, 10, 9 * 16);
	private readonly BuildingZone _residentialZone = new("buildings-residential", 3, 3, int.MaxValue);

	public IReadOnlyList<BuildingZone> Zones => [_centralZone, _urbanZone, _residentialZone];

	public BuildingZone GetZoneByDistance(double distance)
	{
		foreach (var buildingZone in Zones)
		{
			if (distance <= buildingZone.MaxDistanceFromCenter)
			{
				return buildingZone;
			}
		}

		return _residentialZone;
	}
}
