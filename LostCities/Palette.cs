using System.Text.Json.Serialization;
using fNbt;
using Newtonsoft.Json;
using schematic_to_lost_cities.Schematic;

namespace schematic_to_lost_cities.LostCities;

public class Palette
{
	[JsonPropertyName("palette")] public List<PaletteEntry> palette => Items.Values.ToList();
	
	[System.Text.Json.Serialization.JsonIgnore]
	private Dictionary<int, PaletteEntry> Items { get; }

	[System.Text.Json.Serialization.JsonIgnore] 
	private char _currentChar = 'À';

	public Palette()
	{
		Items = new Dictionary<int, PaletteEntry>();
	}

	public void Populate(ConsolidatedPalette consolidatedPalette)
	{
		for (var index = 0; index < consolidatedPalette.Palette.Length; index++)
		{
			var item = consolidatedPalette.Palette[index];
			Items.Add(index, new PaletteEntry
			{
				character = _currentChar++,
				block = item.Get<NbtString>("Name").Value
			});
		}
	}

	public char GetCharacter(int index)
	{
		return Items[index].character;
	}

	public void Serialize()
	{
		var paletteDir = "output/data/lostcities/lostcities/palettes";
		if (!Directory.Exists(paletteDir))
		{
			Directory.CreateDirectory(paletteDir);
		}

		File.WriteAllText($"{paletteDir}/schematic-palette.json", JsonConvert.SerializeObject(this, Formatting.Indented));
	}
}