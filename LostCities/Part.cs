namespace schematic_to_lost_cities.LostCities;

public class Part
{
	private PartFloor[] _floors;

	public Part(Schematic.Schematic schematic, Palette palette)
	{
		var height = schematic.Blocks.GetLength(0);
		var floors = (height / PartFloor.SLICES_PER_FLOOR ) + 1;
		_floors = new PartFloor[floors];

		for (var i = 0; i < floors; i++)
		{
			_floors[i] = new PartFloor();
			for (var j = 0; j < PartFloor.SLICES_PER_FLOOR; j++)
			{
			 _floors[i]
				 .AddLayer(j, schematic.Blocks, (i * PartFloor.SLICES_PER_FLOOR) + j, palette);
			}
		}
	}
}