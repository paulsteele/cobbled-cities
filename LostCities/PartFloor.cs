using System.Text;
using System.Text.Json.Serialization;
using fNbt;

namespace schematic_to_lost_cities.LostCities;

public class PartFloor
{
	[JsonPropertyName("slices")] 
	public string[,] slices;

	[JsonIgnore]
	public static int SLICES_PER_FLOOR = 6;

	public PartFloor()
	{
		slices = new string[SLICES_PER_FLOOR, 16];
	}

	public void AddLayer(int layer, int[,,] id, int y, Palette translation)
	{
		for (var x = 0; x < id.GetLength(1); x++)
		{
			var builder = new StringBuilder();
			for (var z = 0; z < id.GetLength(2); z++)
			{
				builder.Append(
					id.GetLength(0) <= y 
					? ' ' 
					: translation.GetCharacter(id[y, x, z])
				);
			}

			slices[layer, x] = builder.ToString();
		}
	}
}