using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder;

public abstract class AbstractSection
{
	private const int EmptySpaceExtraHeight = 16;
	protected readonly NbtCompound RootTag;
	protected readonly bool[,] TileMap;

	protected AbstractSection(NbtCompound rootTag)
	{
		RootTag = (NbtCompound)rootTag.Clone();
		var blocks = RootTag.Get<NbtList>("blocks");

		if (blocks == null)
		{
			throw new ArgumentException($"{nameof(RootTag)} does not have any blocks");
		}

		TileMap = InitBlocks(blocks);
		InitPalette(RootTag);
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
		for (var z = 0; z < MaxZ; z++)
		{
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
}