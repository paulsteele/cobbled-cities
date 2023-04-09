using schematic_to_lost_cities.Schematic;

namespace schematic_to_lost_cities;

// ReSharper disable once UnusedType.Global
public static class Program
{
	// ReSharper disable once UnusedMember.Global
	public static void Main()
	{
		var pokecenter = new Schematic.Schematic("pokecenter.nbt");
		var pokemart = new Schematic.Schematic("pokemart.nbt");

		var schematics = new List<Schematic.Schematic>()
		{
			pokecenter,
			pokemart
		};
		
		var palette = new ConsolidatedPalette(schematics);

		foreach (var schematic in schematics)
		{
			schematic.UpdateToUseConsolidatedPalette(palette);
		}
		
		Console.WriteLine("test");
	}
}