using System.Reflection.Metadata;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

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
		Grid[tile.Location.X, tile.Location.Z] = tile;

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
		Grid[tile.Location.X, tile.Location.Z] = null;
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

	private RoadSection CreateSubSection(IlRect rect)
	{
		var newSection = new RoadSection(rect.Width + 1, rect.Height + 1);

		for (var x = rect.MinPoint.X; x <= rect.MaxPoint.X; x++)
		{
			for (var z = rect.MinPoint.Z; z <= rect.MaxPoint.Z; z++)
			{
				var tile = GetTile(x, z);

				if (tile == null)
				{
					continue;
				}
				
				var newTile = new RoadTile
				{
					Type = tile.Type,
					Location = tile.Location - rect.MinPoint
				};
				
				newSection.AddTile(newTile);
				RemoveTile(tile);
			}
		}

		return newSection;
	}

	private IlRect GetRect(RoadTile jigsaw)
	{
		// Get pass one delta
		var (xChange, zChange) = jigsaw.Type.GetOffsetForTileType();

		if (xChange == zChange)
		{
			throw new ArgumentException("tile was not a jigsaw");
		}

		var path = new List<IlPoint>();

		var oppositeBoundaryCandidate1 = GetBoundaryInDirection(jigsaw.Location.X, jigsaw.Location.Z, xChange, zChange, path);
		var oppositeBoundaryCandidate2 = GetBoundaryInDirection(jigsaw.Location.X, jigsaw.Location.Z, -xChange, -zChange, path);

		var oppositeBoundary = oppositeBoundaryCandidate1.Equals(jigsaw.Location)
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
			minX = Math.Min(jigsaw.Location.X, oppositeBoundary.X);
			maxX = Math.Max(jigsaw.Location.X, oppositeBoundary.X);

			minZ = mins.Max(i => i.Z);
			maxZ = maxes.Min(i => i.Z);
		}
		//vertical
		else if (zChange != 0)
		{
			minZ = Math.Min(jigsaw.Location.Z, oppositeBoundary.Z);
			maxZ = Math.Max(jigsaw.Location.Z, oppositeBoundary.Z);

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

			var candidateTile = GetTile(newX, newZ);

			if (candidateTile == null)
			{
				return new IlPoint(startingX, startingZ);
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
					return new IlPoint(startingX, startingZ);
			}
			
			trace?.Add(new IlPoint(startingX, startingZ));

			startingX = newX;
			startingZ = newZ;
		}
	}
}