using System.Security.AccessControl;
using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder;

public abstract class AbstractSection
{
	private const int EmptySpaceExtraHeight = 16;
	public readonly NbtCompound RootTag;
	protected readonly bool[,] TileMap;

	protected AbstractSection(NbtCompound rootTag)
	{
		RootTag = (NbtCompound)rootTag.Clone();
		var blocks = RootTag.Get<NbtList>("blocks");

		if (blocks == null)
		{
			throw new ArgumentException($"{nameof(RootTag)} does not have any blocks");
		}

		InitPalette(RootTag);
		ConvertBuildingBlocksToJigsaws(RootTag, blocks);
		TileMap = InitBlocks(blocks);
		InitJigsaws();
	}

	protected int MaxX => RootTag.GetNbtDimensions().x;
	protected int MaxY => RootTag.GetNbtDimensions().y;
	protected int MaxZ => RootTag.GetNbtDimensions().z;
	public Dictionary<IlPoint, Jigsaw.Jigsaw> Jigsaws { get; } = new();
	
	private void InitPalette(NbtCompound rootTag)
	{
		var palette = rootTag.GetPalette();

		var stateDictionary = rootTag.GetTypeStateIds();

		var neededTypes = new[]
		{
			JigsawTileType.North,
			JigsawTileType.East,
			JigsawTileType.South,
			JigsawTileType.West
		};

		foreach (var type in neededTypes)
		{
			if (stateDictionary.ContainsKey(type))
			{
				continue;
			}
			
			palette.Add(CreateJigsaw(type));
		}
	}

	private void InitJigsaws()
	{
		foreach (var jigsaw in Jigsaws.Values)
		{
			var point = jigsaw.TileType.GetOffsetForTileType();
			var candidateLocation = new IlPoint(jigsaw.Location.X + point.x, jigsaw.Location.Z + point.z);

			if (Jigsaws.ContainsKey(candidateLocation))
			{
				jigsaw.PointingToLocation = candidateLocation;
			}

			if (
				candidateLocation.X < 0 ||
				candidateLocation.Z < 0 ||
				candidateLocation.X >= MaxX ||
				candidateLocation.Z >= MaxZ
			)
			{
				jigsaw.PointsToOutside = true;
			}
			
			var reverseCandidateLocation = new IlPoint(jigsaw.Location.X - point.x, jigsaw.Location.Z - point.z);

			if (
				reverseCandidateLocation.X < 0 ||
				reverseCandidateLocation.Z < 0 ||
				reverseCandidateLocation.X >= MaxX ||
				reverseCandidateLocation.Z >= MaxZ
			)
			{
				jigsaw.PointsFromOutside = true;
			}
		}
	}

	protected bool HasTile(int x, int z)
	{
		if (x < 0 || z < 0)
		{
			return false;
		}

		if (x >= MaxX || z >= MaxZ)
		{
			return false;
		}

		return TileMap[x, z];
	}


	private bool[,] InitBlocks(NbtList blocks)
	{
		var tiles = new bool[MaxX, MaxZ];

		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var (posX, _, posZ) = compound.GetNbtPosition();

			tiles[posX, posZ] = true;

			if (!compound.IsJigsaw())
			{
				continue;
			}

			var ilPoint = new IlPoint(posX, posZ);
			Jigsaws.Add(ilPoint, new Jigsaw.Jigsaw(compound, RootTag, ilPoint));
		}

		return tiles;
	}

	// ReSharper disable once UnusedMember.Global
	public void DebugPrint()
	{
		Console.WriteLine("======================================");

		// Print X-axis header (hex: 0-9, A-F)
		Console.Write("     ");
		for (var x = 0; x < MaxX; x++)
		{
			Console.Write($"{x % 16:X}");
		}
		Console.WriteLine();

		// Print separator
		Console.Write("   +-");
		Console.WriteLine(new string('-', MaxX));

		// Print grid with Z-axis labels
		for (var z = 0; z < MaxZ; z++)
		{
			Console.Write($"{z,2} | ");
			for (var x = 0; x < MaxX; x++)
			{
				if (!TileMap[x, z])
				{
					Console.Write(' ');
					continue;
				}

				if (Jigsaws.TryGetValue(new IlPoint(x, z), out var jigsaw))
				{
					var display = jigsaw.IsBuilding ? 'B' : jigsaw.TileType switch
					{
						JigsawTileType.North => '^',
						JigsawTileType.East => '>',
						JigsawTileType.South => 'v',
						JigsawTileType.West => '<',
						_ => throw new ArgumentException($"{nameof(JigsawTileType)}: {jigsaw.TileType} unknown")
					};
					Console.Write(display);

					continue;
				}

				Console.Write("x");

			}

			Console.WriteLine();
		}

		// Print jigsaw details table
		const int jigsawColumnWidth = 50;
		var jigsawColumnDashes = new string('-', jigsawColumnWidth);

		Console.WriteLine("--------------------------------------");
		Console.WriteLine($"Jigsaws ({Jigsaws.Count} total):");
		Console.WriteLine($"Location   | TileType | {"Name",-jigsawColumnWidth} | {"Pool",-jigsawColumnWidth} | Target");
		Console.WriteLine($"-----------+----------+{jigsawColumnDashes}--+{jigsawColumnDashes}--+{jigsawColumnDashes}");

		foreach (var (location, jigsaw) in Jigsaws.OrderBy(j => j.Key.Z).ThenBy(j => j.Key.X))
		{
			var locationStr = $"({location.X}, {location.Z})".PadRight(10);
			var tileTypeStr = (jigsaw.IsBuilding ? "Building" : jigsaw.TileType.ToString()).PadRight(8);
			var nameStr = TruncateOrPad(jigsaw.GetJigsawName(), jigsawColumnWidth);
			var poolStr = TruncateOrPad(jigsaw.GetJigsawPool(), jigsawColumnWidth);
			var targetStr = TruncateOrPad(jigsaw.GetJigsawTarget(), jigsawColumnWidth);

			Console.WriteLine($"{locationStr} | {tileTypeStr} | {nameStr} | {poolStr} | {targetStr}");
		}
	}

	private static string TruncateOrPad(string value, int length)
	{
		if (string.IsNullOrEmpty(value))
		{
			return new string(' ', length);
		}

		return value.Length > length ? value[..length] : value.PadRight(length);
	}
	
	public void FillEmptySpace()
	{
		var palette = RootTag.GetPalette();

		var caveAirIndex = palette.Count;
		palette.Add(new NbtCompound(new List<NbtTag>
		{
			new NbtString("Name", "minecraft:cave_air")
		}));
		
		var blocks = RootTag.Get<NbtList>("blocks");

		if (blocks == null)
		{
			return;
		}

		blocks = (NbtList) blocks.Clone();
		
		RootTag.SetNbtDimensions(MaxX, MaxY + EmptySpaceExtraHeight, MaxZ);
		
		var blockMatrix = new NbtCompound?[MaxX, MaxZ, MaxY];
		
		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var (posX, posY, posZ) = compound.GetNbtPosition();
			blockMatrix[posX, posZ, posY] = compound;
		}
		
		for (var x = 0; x < MaxX; x++)
		{
			for (var z = 0; z < MaxZ; z++)
			{
				for (var y = 0; y < MaxY; y++)
				{
					blockMatrix[x, z, y] ??= new NbtCompound
					{
						["pos"] = new NbtList("pos") { new NbtInt(x), new NbtInt(y), new NbtInt(z) },
						["state"] = new NbtInt("state", caveAirIndex)
					};
				}
			}
		}
		
		var newBlocks = new NbtList("blocks");

		foreach (var block in blockMatrix)
		{
			if (block == null)
			{
				continue;
			}
			
			newBlocks.Add(block);
		}
		
		RootTag["blocks"] = newBlocks;
	}
	
	private NbtCompound CreateJigsaw(JigsawTileType orientation)
	{
		var name = new NbtString("Name", "minecraft:jigsaw");
		var orientationNode = new NbtString("orientation", orientation.GetGameName());
		var properties = new NbtCompound("Properties", new[] { orientationNode });
		return new NbtCompound(new List<NbtTag> {properties, name});
	}

	private void ConvertBuildingBlocksToJigsaws(NbtCompound rootTag, NbtList blocks)
	{
		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var name = compound.GetPaletteTag(rootTag, "Name");

			if (
				name.StartsWith("cobbledcitiesblocks:building_block") ||
				name.StartsWith("cobbledcitiesblocks:long_building_block") ||
				name.StartsWith("cobbledcitiesblocks:corner_building_block")
				)
			{
				compound.MakeJigsaw();
			}
		}
	}
	
	public void RotateBuildingJigsaws()
	{
		var states = RootTag.GetTypeStateIds();

		foreach (var jigsaw in Jigsaws.Values.Where(j => j.IsBuilding))
		{
			var pos = jigsaw.Location;
			
			var maxX = MaxX - 1;
			var maxZ = MaxZ - 1;
			
			var tileType = pos switch
			{
				{ X: 0, Z: 0 } => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == 0 && pos.Z == maxZ => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == maxX && pos.Z == 0 => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == maxX && pos.Z == maxZ => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == 0 => JigsawTileType.West,
				_ when pos.X == maxX => JigsawTileType.East,
				_ when pos.Z == 0 => JigsawTileType.North,
				_ when pos.Z == maxZ => JigsawTileType.South,
				_ => throw new ArgumentException($"Building jigsaw at {pos} not on edge")
			};

			jigsaw.TileType = tileType;
			jigsaw.Compound.SetState(states[tileType]);
		}
	}

}
