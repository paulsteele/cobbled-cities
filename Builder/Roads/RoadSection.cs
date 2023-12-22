using fNbt;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public class RoadSection
{
	private readonly NbtCompound _rootTag;
	public Dictionary<IlPoint, Jigsaw> Jigsaws { get; } = new();

	private int MaxX => _rootTag.GetNbtDimensions().x;
	private int MaxY => _rootTag.GetNbtDimensions().y;
	private int MaxZ => _rootTag.GetNbtDimensions().z;

	private readonly bool[,] _hasTile;
	public int Index { get; }
	private int nextSubsectionIndex = 0;

	public RoadSection(
		NbtCompound rootTag, 
		IlRect? boundingBox = null, 
		Dictionary<IlPoint, Jigsaw>? rootJigsaws = null,
		int index = -1
	)
	{
		_rootTag = (NbtCompound) rootTag.Clone();
		
		var blocks = _rootTag.Get<NbtList>("blocks");
		
		if (blocks == null)
		{
			throw new ArgumentException($"{nameof(_rootTag)} does not have any blocks");
		}

		blocks = InitBoundingBox(boundingBox, blocks);
		_hasTile = InitBlocks(blocks);
		InitJigsaws(rootJigsaws, boundingBox);
		Index = index;
	}

	private NbtList InitBoundingBox(IlRect? boundingBox, NbtList blocks)
	{
		if (boundingBox == null)
		{
			return blocks;
		}
		
		_rootTag.SetNbtDimensions(boundingBox.Width + 1, MaxY, boundingBox.Height + 1);

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

		_rootTag["blocks"] = newList;
		return newList;
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

			var (posX, _,  posZ) = compound.GetNbtPosition();

			tiles[posX, posZ] = true;

			if (!compound.IsJigsaw())
			{
				continue;
			}

			var ilPoint = new IlPoint(posX, posZ);
			Jigsaws.Add(ilPoint, new Jigsaw(compound, _rootTag, ilPoint));
		}

		return tiles;
	}

	private void InitJigsaws(Dictionary<IlPoint, Jigsaw>? rootJigsaws, IlRect? boundingBox)
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

			}
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
				
				if (rootJigsaws.TryGetValue(jigsawsValue.OriginalLocation, out var rootJigsaw))
				{
					jigsawsValue.PointingToLocation = rootJigsaw.PointingToLocation;
				}
			}
		}
	}

	private bool HasTile(int x, int z)
	{
		if (x < 0 || z < 0)
		{
			return false;
		}

		if (x >= MaxX || z >= MaxZ)
		{
			return false;
		}

		return _hasTile[x, z];
	}

	public void DebugPrint()
	{
		Console.WriteLine("======================================");
		for (var z = 0; z < MaxZ; z++)
		{
			for (var x = 0; x < MaxX; x++)
			{
				if (!HasTile(x, z))
				{
					Console.Write(' ');
					continue;
				}

				if (Jigsaws.TryGetValue(new IlPoint(x, z), out var jigsaw))
				{
					var display = jigsaw.TileType switch
					{
						JigsawTileType.North => '↑',
						JigsawTileType.East => '→',
						JigsawTileType.South => '↓',
						JigsawTileType.West => '←',
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

	public bool HasSubSections => Jigsaws.Count > 0;

	public RoadSection TakeSubSection()
	{
		var first = Jigsaws.First().Value;

		var coordinates = GetRect(first.Compound);

		var subsection = new RoadSection(_rootTag, coordinates, Jigsaws, nextSubsectionIndex++);

		var toRemove = Jigsaws
			.Where(j => coordinates.PointInside(j.Key))
			.Select(j => j.Key)
			.ToList();

		foreach (var point in toRemove)
		{
			Jigsaws.Remove(point);
		}
		
		coordinates.ForEach((x, z) => _hasTile[x, z] = false);

		return subsection;
	}

	public void UpdateJigsaws(string baseFileName, Dictionary<IlPoint, int> jigsawPointToIndex)
	{
		FlipPointedToJigsaws();

		foreach (var jigsaw in Jigsaws.Values)
		{
			jigsaw.SetJigsawName($"poke-cities:{baseFileName}-{Index}-{jigsaw.OriginalLocation.SerializedString}");
			if (
				jigsaw.PointingToLocation == null ||
				!jigsawPointToIndex.TryGetValue(jigsaw.PointingToLocation, out var pointIndex)
			)
			{
				continue;
			}
			jigsaw.SetJigsawPool($"poke-cities:{baseFileName}-{pointIndex}");
			jigsaw.SetJigsawTarget($"poke-cities:{baseFileName}-{pointIndex}-{jigsaw.PointingToLocation.SerializedString}");
		}
	}

	private void FlipPointedToJigsaws()
	{
		var states = _rootTag.GetTypeStateIds();
		foreach (var jigsaw in Jigsaws.Values.Where(jigsaw => jigsaw.PointingToLocation == null))
		{
			jigsaw.Flip(states);
		}
	}

	private IlRect GetRect(NbtCompound jigsaw)
	{
		// Get pass one delta
		var (xChange, zChange) = jigsaw.GetJigsawTileType(_rootTag).GetOffsetForTileType();

		if (xChange == zChange)
		{
			throw new ArgumentException("tile was not a jigsaw");
		}

		var path = new List<IlPoint>();

		var location = jigsaw.GetNbtPosition();

		var oppositeBoundaryCandidate1 = GetBoundaryInDirection(location.x, location.z, xChange, zChange, path);
		var oppositeBoundaryCandidate2 = GetBoundaryInDirection(location.x, location.z, -xChange, -zChange, path);

		var oppositeBoundary = oppositeBoundaryCandidate1.Equals(new IlPoint(location.x, location.z))
			? oppositeBoundaryCandidate2
			: oppositeBoundaryCandidate1;
		
		path.Add(oppositeBoundary);

		var crossXChange = Math.Abs(zChange);
		var crossZChange = Math.Abs(xChange);

		var mins = path.Select(i => GetBoundaryInDirection(i.X, i.Z, -crossXChange, -crossZChange));
		var maxes = path.Select(i => GetBoundaryInDirection(i.X, i.Z, crossXChange, crossZChange));

		var minX = 0;
		var minZ = 0;
		var maxX = 0;
		var maxZ = 0;
		
		//horizontal
		if (xChange != 0)
		{
			minX = Math.Min(location.x, oppositeBoundary.X);
			maxX = Math.Max(location.x, oppositeBoundary.X);

			minZ = mins.Max(i => i.Z);
			maxZ = maxes.Min(i => i.Z);
		}
		//vertical
		else if (zChange != 0)
		{
			minZ = Math.Min(location.z, oppositeBoundary.Z);
			maxZ = Math.Max(location.z, oppositeBoundary.Z);

			minX = mins.Max(i => i.X);
			maxX = maxes.Min(i => i.X);
		}

		return new IlRect(minX, minZ, maxX, maxZ);
	}

	private IlPoint GetBoundaryInDirection(int startingX, int startingZ, int offsetX, int offsetZ, ICollection<IlPoint>? trace = null)
	{
		var allowedToTakeJigsaw = true;
		while (true)
		{
			var newX = startingX + offsetX;
			var newZ = startingZ + offsetZ;

			if (!HasTile(newX, newZ))
			{
				return new IlPoint(startingX, startingZ);
			}

			if (Jigsaws.TryGetValue(new IlPoint(newX, newZ), out var jigsaw))
			{
				switch (jigsaw.TileType)
				{
					case JigsawTileType.North when allowedToTakeJigsaw:
					case JigsawTileType.East when allowedToTakeJigsaw:
					case JigsawTileType.South when allowedToTakeJigsaw:
					case JigsawTileType.West when allowedToTakeJigsaw:
						allowedToTakeJigsaw = false;
						break;
					default:
						return new IlPoint(startingX, startingZ);
				}
			}

			trace?.Add(new IlPoint(startingX, startingZ));

			startingX = newX;
			startingZ = newZ;
		}
	}

	public bool IsCenter()
	{
		return Jigsaws.Values.All(j => j.PointingToLocation != null);
	}
}