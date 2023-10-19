namespace Minecraft.City.Datapack.Generator.Tiling.Roads;

public class RoadTile
{
	public RoadTileType Type { get; set; }
	public int X { get; set; }
	public int Z { get; set; }
}

public enum RoadTileType
{
	Empty,
	Filled,
	North,
	East,
	South,
	West
}