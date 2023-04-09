using System.Text.Json.Serialization;

namespace schematic_to_lost_cities.LostCities;

public class PaletteEntry
{
	[JsonPropertyName("char")]
	public char character { get; set; }
	
	[JsonPropertyName("block")]
	public string block { get; set; }
}