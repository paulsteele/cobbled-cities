namespace Minecraft.City.Datapack.Generator.Tiling.Roads;

public class RoadSection
{
	private RoadTile?[,] Grid { get; }

	private List<RoadTile> Jigsaws { get; } = new();

	public RoadSection(int x, int z)
	{
		Grid = new RoadTile[x, z];
	}

	public void AddTile(RoadTile tile)
	{
		Grid[tile.X, tile.Z] = tile;

		if (tile.Type is RoadTileType.East or RoadTileType.North or RoadTileType.West or RoadTileType.South)
		{
			Jigsaws.Add(tile);
		}
	}

	private RoadTile? GetTile(int x, int z)
	{
		if (x < 0 || z < 0)
		{
			return null;
		}

		if (x >= Grid.GetLength(0))
		{
			return null;
		}

		if (z >= Grid.GetLength(1))
		{
			return null;
		}

		return Grid[x, z];
	}

	private void RemoveTile(RoadTile tile)
	{
		Jigsaws.Remove(tile);
		Grid[tile.X, tile.Z] = null;
	}
	
	public void DebugPrint()
	{
		Console.WriteLine("======================================");
		for (var z = 0; z < Grid.GetLength(1); z++)
		{
			for (var x = 0; x < Grid.GetLength(0); x++)
			{
				var tile = GetTile(x, z);

				if (tile == null)
				{
					Console.Write(' ');
					continue;
				}

				var display = tile.Type switch
				{
					RoadTileType.Filled => 'x',
					RoadTileType.North => '↑',
					RoadTileType.East => '→',
					RoadTileType.South => '↓',
					RoadTileType.West => '←',
					_ => ' '
				};
				Console.Write(display);
			}
			Console.WriteLine();
		}
	}

	public bool HasSubSections => Jigsaws.Count > 0;

	public RoadSection TakeSubSection()
	{
		var first = Jigsaws.First();

		var coordinates = GetRect(first);

		var subsection = CreateSubSection(coordinates);

		return subsection;
	}

	private RoadSection CreateSubSection((int minX, int minZ, int maxX, int maxZ) coordinates)
	{
		var width = coordinates.maxX - coordinates.minX + 1;
		var height = coordinates.maxZ - coordinates.minZ + 1;

		var newSection = new RoadSection(width, height);

		for (var x = coordinates.minX; x <= coordinates.maxX; x++)
		{
			for (int z = coordinates.minZ; z <= coordinates.maxZ; z++)
			{
				var tile = GetTile(x, z);

				if (tile == null)
				{
					continue;
				}
				
				var newTile = new RoadTile
				{
					Type = tile.Type,
					X = tile.X - coordinates.minX,
					Z = tile.Z - coordinates.minZ
				};
				
				newSection.AddTile(newTile);
				RemoveTile(tile);
			}
		}

		return newSection;
	}

	private (int minX, int minZ, int maxX, int maxZ) GetRect(RoadTile jigsaw)
	{
		// Get pass one delta
		var (xChange, zChange) = jigsaw.Type.GetOffsetForTileType();

		if (xChange == zChange)
		{
			throw new ArgumentException("tile was not a jigsaw");
		}

		(xChange, zChange) = (Math.Abs(xChange), Math.Abs(zChange));

		var directionMin = GetBoundaryInDirection(jigsaw.X, jigsaw.Z, -xChange, -zChange);
		var directionMax = GetBoundaryInDirection(jigsaw.X, jigsaw.Z, xChange, zChange);

		// switch to get cross sections
		(xChange, zChange) = (zChange, xChange);

		var directionMinCrossMin = GetBoundaryInDirection(directionMin.x, directionMin.z, -xChange, -zChange);
		var directionMinCrossMax = GetBoundaryInDirection(directionMin.x, directionMin.z, xChange, zChange);
		
		var directionMaxCrossMin = GetBoundaryInDirection(directionMax.x, directionMax.z, -xChange, -zChange);
		var directionMaxCrossMax = GetBoundaryInDirection(directionMax.x, directionMax.z, xChange, zChange);
		
		//switch back to make logic more "intuitive"
		(xChange, zChange) = (zChange, xChange);

		var minX = 0;
		var minZ = 0;
		var maxX = 0;
		var maxZ = 0;

		// horizontal
		if (xChange > 0)
		{
			minX = directionMin.x;
			maxX = directionMax.x;

			minZ = Math.Max(directionMinCrossMin.z, directionMaxCrossMin.z);
			maxZ = Math.Min(directionMinCrossMax.z, directionMaxCrossMax.z);
		}

		// vertical
		if (zChange > 0)
		{
			minZ = directionMin.z;
			maxZ = directionMax.z;

			minX = Math.Max(directionMinCrossMin.x, directionMaxCrossMin.x);
			maxX = Math.Min(directionMinCrossMax.x, directionMaxCrossMax.x);
		}
		
		return (minX, minZ, maxX, maxZ);
	}

	private (int x, int z) GetBoundaryInDirection(int startingX, int startingZ, int offsetX, int offsetZ)
	{
		var allowedToTakeJigsaw = true;
		while (true)
		{
			var newX = startingX + offsetX;
			var newZ = startingZ + offsetZ;

			var candidateTile = GetTile(newX, newZ);

			if (candidateTile == null)
			{
				return (startingX, startingZ);
			}

			switch (candidateTile.Type)
			{
				case RoadTileType.North when allowedToTakeJigsaw:
				case RoadTileType.East when allowedToTakeJigsaw:
				case RoadTileType.South when allowedToTakeJigsaw:
				case RoadTileType.West when allowedToTakeJigsaw:
					allowedToTakeJigsaw = false;
					break;
				case RoadTileType.Filled:
					break;
				case RoadTileType.Empty:
				default:
					return (startingX, startingZ);
			}

			startingX = newX;
			startingZ = newZ;
		}
	}
}