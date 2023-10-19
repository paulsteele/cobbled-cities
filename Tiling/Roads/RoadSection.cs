using System.Security;

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
	
	public void DebugPrint()
	{
		for (var i = 0; i < Grid.GetLength(0); i++)
		{
			for (var j = 0; j < Grid.GetLength(1); j++)
			{
				if (Grid[j, i] == null)
				{
					Console.Write(' ');
					continue;
				}
				var display = Grid[j, i]!.Type switch
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
		var starting = Jigsaws.First();

		if (starting.Type == RoadTileType.North || starting.Type == RoadTileType.South)
		{
			var top = GetBoundaryInDirection(starting.X, starting.Z, 0, -1);
			var bottom = GetBoundaryInDirection(starting.X, starting.Z, 0, 1);
			
			Console.WriteLine(top);
			Console.WriteLine(bottom);
		}

		return this;
	}

	private (int x, int z) GetBoundaryInDirection(int startingX, int startingZ, int offsetX, int offsetZ)
	{
		var newX = startingX + offsetX;
		var newZ = startingZ + offsetZ;

	}
}