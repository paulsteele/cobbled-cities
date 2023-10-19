namespace Minecraft.City.Datapack.Generator.NbtHandler;

public class GridSection
{
	public GridSectionTile[,] Grid { get; set; } = new GridSectionTile[0, 0];

	public void DebugPrint()
	{
		for (var i = 0; i < Grid.GetLength(0); i++)
		{
			for (var j = 0; j < Grid.GetLength(1); j++)
			{
				var display = Grid[j, i] switch
				{
					GridSectionTile.Filled => 'x',
					GridSectionTile.North => '↑',
					GridSectionTile.East => '→',
					GridSectionTile.South => '↓',
					GridSectionTile.West => '←',
					_ => ' '
				};
				Console.Write(display);
			}
			Console.WriteLine();
		}
	}

	public GridSection TakeSection()
	{
		return this;
	}
}