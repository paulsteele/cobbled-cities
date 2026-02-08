using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Jigsaw;

public enum JigsawTileType
{
	North,
	East,
	South,
	West,
	BuildingNormal,
	BuildingCorner,
	BuildingLong,
	BuildingLongExtension
}

public static class JigsawTileTypeExtensions
{
	public static JigsawTileType GetJigsawTileType(this NbtCompound compound, NbtCompound rootTag)
	{
		var orientation = compound.GetPalettePropertiesTag(rootTag,"orientation");

		return orientation switch
		{
			"north_up" => JigsawTileType.North,
			"south_up" => JigsawTileType.South,
			"west_up" => JigsawTileType.West,
			"east_up" => JigsawTileType.East,
			_ => GetJigsawBuildingType(compound, rootTag),
		};
	}
	
	private static JigsawTileType GetJigsawBuildingType(NbtCompound compound, NbtCompound rootTag)
	{
		var name = compound.GetPaletteTag(rootTag, "Name");
		return name switch
		{
			"cobbledcitiesblocks:building_block" => JigsawTileType.BuildingNormal,
			"cobbledcitiesblocks:long_building_block" => JigsawTileType.BuildingLong,
			"cobbledcitiesblocks:corner_building_block" => JigsawTileType.BuildingCorner,
			_ => throw new ArgumentOutOfRangeException($"could not determine {nameof(JigsawTileType)} from {name} {compound}")
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
					dictionary.TryAdd(JigsawTileType.North, index);
					break;
				case "south_up":
					dictionary.TryAdd(JigsawTileType.South, index);
					break;
				case "east_up":
					dictionary.TryAdd(JigsawTileType.East, index);
					break;
				case "west_up":
					dictionary.TryAdd(JigsawTileType.West, index);
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
	
	public static JigsawTileType Rotated180DegreesTileType(this JigsawTileType tileType)
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

	public static JigsawTileType Rotated90DegreesClockwiseTileType(this JigsawTileType tileType)
	{
		return tileType switch
		{
			JigsawTileType.North => JigsawTileType.East,
			JigsawTileType.East => JigsawTileType.South,
			JigsawTileType.South => JigsawTileType.West,
			JigsawTileType.West => JigsawTileType.North,
			_ => throw new ArgumentException($"Invalid {nameof(JigsawTileType)} {tileType}")
		};
	}

	public static string GetGameName(this JigsawTileType tileType)
	{
		return tileType switch
		{
			JigsawTileType.North => "north_up",
			JigsawTileType.East => "east_up",
			JigsawTileType.South => "south_up",
			JigsawTileType.West => "west_up",
			_ => throw new ArgumentOutOfRangeException($"{nameof(tileType)}: {tileType} not supported value")
		};
	}

	public static readonly JigsawTileType[] BuildingTypes = 
	[
		JigsawTileType.BuildingNormal,
		JigsawTileType.BuildingCorner,
		JigsawTileType.BuildingLong
	];
	
	public static string GetBuildingTypeFolderName(this JigsawTileType tileType)
	{
		return tileType.ToString().ToLower().Replace("building", string.Empty);
	}
}
