using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder;

public abstract class AbstractSection
{
	private const int EmptySpaceExtraHeight = 16;
	protected readonly NbtCompound RootTag;
	protected readonly bool[,] TileMap;

	protected AbstractSection(
		NbtCompound rootTag,
		IlRect? boundingBox = null,
		Dictionary<IlPoint, Jigsaw.Jigsaw>? rootJigsaws = null
	)
	{
		RootTag = (NbtCompound)rootTag.Clone();
		var blocks = RootTag.Get<NbtList>("blocks");

		if (blocks == null)
		{
			throw new ArgumentException($"{nameof(RootTag)} does not have any blocks");
		}

		InitPalette(boundingBox, RootTag);
		blocks = InitBoundingBox(boundingBox, blocks);
		TileMap = InitBlocks(blocks);
		InitJigsaws(rootJigsaws, boundingBox);
	}

	protected int MaxX => RootTag.GetNbtDimensions().x;
	protected int MaxY => RootTag.GetNbtDimensions().y;
	protected int MaxZ => RootTag.GetNbtDimensions().z;
	public Dictionary<IlPoint, Jigsaw.Jigsaw> Jigsaws { get; } = new();
	
	private void InitPalette(IlRect? boundingBox, NbtCompound rootTag)
	{
		if (boundingBox != null)
		{
			return;
		}

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
	
	private NbtList InitBoundingBox(IlRect? boundingBox, NbtList blocks)
	{
		if (boundingBox == null)
		{
			return blocks;
		}

		RootTag.SetNbtDimensions(boundingBox.Width + 1, MaxY, boundingBox.Height + 1);

		var tempBlocks = blocks
			.Where(b => b is NbtCompound)
			.Cast<NbtCompound>()
			.Where(b =>
			{
				var pos = b.GetNbtPosition();
				return boundingBox.PointInside(pos.x, pos.z);
			}).ToList();

		foreach (var block in tempBlocks)
		{
			var pos = block.GetNbtPosition();
			block.SetNbtPosition(pos.x - boundingBox.MinPoint.X, pos.y, pos.z - boundingBox.MinPoint.Z);
		}

		var newList = new NbtList("blocks");
		newList.AddRange(tempBlocks);

		RootTag["blocks"] = newList;
		return newList;
	}

	private void InitJigsaws(Dictionary<IlPoint, Jigsaw.Jigsaw>? rootJigsaws, IlRect? boundingBox)
	{
		if (rootJigsaws == null)
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

			MarkBuildingJigsaws();
		}
		else
		{
			foreach (var jigsawsValue in Jigsaws.Values)
			{
				if (boundingBox != null)
				{
					jigsawsValue.OriginalLocation = new IlPoint(
						jigsawsValue.Location.X + boundingBox.MinPoint.X,
						jigsawsValue.Location.Z + boundingBox.MinPoint.Z
					);
				}

				if (!rootJigsaws.TryGetValue(jigsawsValue.OriginalLocation, out var rootJigsaw))
				{
					continue;
				}
				jigsawsValue.PointingToLocation = rootJigsaw.PointingToLocation;
				jigsawsValue.PointsToOutside = rootJigsaw.PointsToOutside;
				jigsawsValue.PointsFromOutside = rootJigsaw.PointsFromOutside;
				jigsawsValue.IsBuilding = rootJigsaw.IsBuilding;
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

	private void MarkBuildingJigsaws()
	{
		var pointedTo = new HashSet<IlPoint>(Jigsaws.Values
			.Where(j => j is { PointingToLocation: not null, PointsToOutside: false })
			.Select(j => j.PointingToLocation!));

		foreach (var jigsaw in Jigsaws.Values)
		{
			var hasNoOutgoing = jigsaw.PointingToLocation == null;
			var notPointedTo = !pointedTo.Contains(jigsaw.Location);
			var notPointingOutside = !jigsaw.PointsToOutside;

			jigsaw.IsBuilding = hasNoOutgoing && notPointedTo && notPointingOutside;
		}
	}
}