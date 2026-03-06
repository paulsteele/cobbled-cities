using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings.Palette;

public static class PaletteReplacer
{
	private const string PalettePrefix = "cobbledcitiesblocks:palette_";

	private static readonly Dictionary<string, (char family, string variant)> PaletteBlockLookup = BuildLookup();

	private static Dictionary<string, (char family, string variant)> BuildLookup()
	{
		var lookup = new Dictionary<string, (char family, string variant)>();
		foreach (var family in new[] { 'a', 'b', 'c', 'd' })
		{
			lookup[$"cobbledcitiesblocks:palette_block_{family}"] = (family, "block");
			lookup[$"cobbledcitiesblocks:palette_stairs_{family}"] = (family, "stairs");
			lookup[$"cobbledcitiesblocks:palette_slab_{family}"] = (family, "slab");
			lookup[$"cobbledcitiesblocks:palette_wall_{family}"] = (family, "wall");
			lookup[$"cobbledcitiesblocks:palette_fence_{family}"] = (family, "fence");
			lookup[$"cobbledcitiesblocks:palette_fence_gate_{family}"] = (family, "fence_gate");
			lookup[$"cobbledcitiesblocks:palette_pane_{family}"] = (family, "pane");
		}
		return lookup;
	}

	public static void Apply(NbtCompound rootTag, PaletteCombination combination)
	{
		var palette = rootTag.GetPalette();

		foreach (var entry in palette)
		{
			if (entry is not NbtCompound paletteEntry)
			{
				continue;
			}

			var nameTag = paletteEntry.Get<NbtString>("Name");
			if (nameTag == null)
			{
				continue;
			}

			var name = nameTag.Value;
			if (!name.StartsWith(PalettePrefix))
			{
				continue;
			}

			if (!PaletteBlockLookup.TryGetValue(name, out var info))
			{
				continue;
			}

			var mapping = combination.GetMappingForFamily(info.family);
			if (mapping == null)
			{
				continue;
			}

			var replacement = mapping.GetVariant(info.variant);
			paletteEntry["Name"] = new NbtString("Name", replacement);
		}
	}
}
