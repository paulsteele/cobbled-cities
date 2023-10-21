namespace Minecraft.City.Datapack.Generator.Tiling.Roads;

public enum RoadTileType
{
	Empty,
	Filled,
	North,
	East,
	South,
	West
}

public static class RoadTileTypeExtensions
{
	public static (int x, int z) GetOffsetForTileType(this RoadTileType tileType)
	{
		return tileType switch
		{
			RoadTileType.Empty => (-1, -1),
			RoadTileType.Filled => (-1, -1),
			RoadTileType.North => (0, -1),
			RoadTileType.East => (1, 0),
			RoadTileType.South => (0, 1),
			RoadTileType.West => (-1, 0),
			_ => (-1, -1)
		};
	}
}