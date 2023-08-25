using System.Text;
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
				@char = _currentChar++,
				block = GenerateName(item)
			});
		}
	}

	private string GenerateName(NbtCompound item)
	{
		var name = item.Get<NbtString>("Name")?.Value;

		var list = new string[]
		{
			"quark",
			"cobblemon",
			"createdeco",
			"supplementaries"
		};
		
		
		var properties = item.Get<NbtCompound?>("Properties");

		if (list.All(l => !name.Contains(l)))
		{
			if (properties != null)
			{
				var props = properties
					.Where(tag => tag.Name != "waterlogged")
					.Select(tag => $"\"{tag.Name}\"=\"{tag.StringValue}\"").ToList();

				if (props.Count > 1)
				{
					name = $"{name}[{string.Join(',', props)}]";
				}
			}
		}

		return name;
	}

	public char GetCharacter(int index)
	{
		return index == -1 ? ' ' : Items[index].@char;
	}

	public void Serialize()
	{
		var paletteDir = "output/data/lostcities/lostcities/palettes";
		if (!Directory.Exists(paletteDir))
		{
			Directory.CreateDirectory(paletteDir);
		}

		File.WriteAllText($"{paletteDir}/schematicpalette.json", JsonConvert.SerializeObject(this, Formatting.Indented));
	}
}