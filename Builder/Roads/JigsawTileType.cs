using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public enum JigsawTileType
{
	North,
	East,
	South,
	West
}

public static class JigsawTileTypeExtensions
{
	public static JigsawTileType GetJigsawTileType(this NbtCompound compound, NbtCompound rootTag)
	{
		var orientation = compound.GetPaletteTag(rootTag,"orientation");

		return orientation switch
		{
			"north_up" => JigsawTileType.North,
			"south_up" => JigsawTileType.South,
			"west_up" => JigsawTileType.West,
			"east_up" => JigsawTileType.East,
			_ => throw new ArgumentOutOfRangeException($"{nameof(orientation)}: {orientation} not supported value")
		};
	}
	
	public static (int x, int z) GetOffsetForTileType(this JigsawTileType tileType)
	{
		return tileType switch
		{
			JigsawTileType.North => (0, -1),
			JigsawTileType.East => (1, 0),
			JigsawTileType.South => (0, 1),
			JigsawTileType.West => (-1, 0),
			_ => (-1, -1)
		};
	}
}