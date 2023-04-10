using Newtonsoft.Json;
using schematic_to_lost_cities.LostCities;
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

		var lostCitiesPalette = new Palette();
		lostCitiesPalette.Populate(palette);

		lostCitiesPalette.Serialize();

		var parts = schematics.Select(s => new Part(s, lostCitiesPalette)).ToList();
		var style = new CityStyle();
		style.SetBuildings(parts);
		style.Serialize();

		var world = new World();
		world.Serialize();
		var writer = new StaticWriter();
		writer.Serialize();

		foreach (var part in parts)
		{
			part.Serialize();
		}
		
		Console.WriteLine("done");
	}
}