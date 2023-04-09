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
		var palette = new ConsolidatedPalette(new[] { pokecenter, pokemart });
		Console.WriteLine("test");
	}
}