namespace Minecraft.City.Datapack.Generator.Services;

public interface IBuildingPoolService
{
	string CentralPoolName { get; }
	string UrbanPoolName { get; }
	string ResidentialPoolName { get; }
	string GetPoolNameForRoadType(string roadTypeName);
}

public class BuildingPoolService : IBuildingPoolService
{
	public string CentralPoolName => "buildings-central";
	public string UrbanPoolName => "buildings-urban";
	public string ResidentialPoolName => "buildings-residential";

	public string GetPoolNameForRoadType(string roadTypeName)
	{
		return roadTypeName switch
		{
			"centers" => CentralPoolName,
			"cardinals" => UrbanPoolName,
			"inters" => ResidentialPoolName,
			_ => throw new ArgumentException($"Unknown road type name: {roadTypeName}")
		};
	}
}