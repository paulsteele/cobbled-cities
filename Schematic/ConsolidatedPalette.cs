using fNbt;

namespace schematic_to_lost_cities.Schematic;

public class ConsolidatedPalette
{
	public readonly NbtCompound[] Palette;

	public ConsolidatedPalette(IEnumerable<Schematic> schematics)
	{
		Palette = CreateConsolidatedPalette(schematics);
	}

	private static NbtCompound[] CreateConsolidatedPalette(IEnumerable<Schematic> schematics)
	{
		var pendingPalette = new HashSet<NbtCompound>();

		foreach (var schematic in schematics)
		{
			for (var index = 0; index < schematic.Palette.Length; index++)
			{
				var paletteItem = schematic.Palette[index];
				if (!pendingPalette.Contains(paletteItem, new PaletteEquality()))
				{
					pendingPalette.Add(paletteItem);
				}
			}
		}

		return pendingPalette.ToArray();
	}
}