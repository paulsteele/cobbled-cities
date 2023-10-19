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

	public RoadSection TakeSubSection()
	{
		return this;
	}
}