namespace Minecraft.City.Datapack.Generator.Tiling.Roads;

public class RoadSection
{
	public RoadTile[,] Grid { get; set; } = new RoadTile[0, 0];

	public void DebugPrint()
	{
		for (var i = 0; i < Grid.GetLength(0); i++)
		{
			for (var j = 0; j < Grid.GetLength(1); j++)
			{
				var display = Grid[j, i] switch
				{
					RoadTile.Filled => 'x',
					RoadTile.North => '↑',
					RoadTile.East => '→',
					RoadTile.South => '↓',
					RoadTile.West => '←',
					_ => ' '
				};
				Console.Write(display);
			}
			Console.WriteLine();
		}
	}

	public RoadSection TakeSection()
	{
		return this;
	}
}