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
	
	public static Dictionary<JigsawTileType, int> GetTypeStateIds(this NbtCompound rootTag)
	{
		var dictionary = new Dictionary<JigsawTileType, int>();

		var list = rootTag.GetPalette().ToArray();
		for (var index = 0; index < list.Length; index++)
		{
			var paletteItem = list[(Index)index] as NbtCompound;
			var orientation = paletteItem?.Get<NbtCompound>("Properties")?.Get<NbtString>("orientation")?.Value;
			if (
				orientation == null ||
				!"minecraft:jigsaw".Equals(paletteItem?.Get<NbtString>("Name")?.Value)
			)
			{
				continue;
			}
			

			switch (orientation)
			{
				case "north_up":
					dictionary.Add(JigsawTileType.North, index);
					break;
				case "south_up":
					dictionary.Add(JigsawTileType.South, index);
					break;
				case "east_up":
					dictionary.Add(JigsawTileType.East, index);
					break;
				case "west_up":
					dictionary.Add(JigsawTileType.West, index);
					break;
			}
		}

		return dictionary;
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
	
	public static JigsawTileType FlippedTileType(this JigsawTileType tileType)
	{
		return tileType switch
		{
			JigsawTileType.North => JigsawTileType.South,
			JigsawTileType.East => JigsawTileType.West,
			JigsawTileType.South => JigsawTileType.North,
			JigsawTileType.West => JigsawTileType.East,
			_ => throw new ArgumentException($"Invalid {nameof(JigsawTileType)} {tileType}")
		};
	}
}