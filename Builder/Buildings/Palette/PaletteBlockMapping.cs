namespace Minecraft.City.Datapack.Generator.Builder.Buildings.Palette;

public record PaletteBlockMapping(
	string Block,
	string Stairs,
	string Slab,
	string Wall,
	string Fence,
	string FenceGate,
	string Pane
)
{
	// Known Minecraft block families: base block name -> (stair prefix, slab prefix, wall prefix)
	// Fence/fence_gate/pane are only available for specific block types and use different conventions.
	// For blocks not in this table, we fall back to {base}_{suffix} convention.
	private static readonly Dictionary<string, BlockFamilyNames> KnownFamilies = new()
	{
		// Stone variants
		["stone"] = new("stone", "stone", "stone"),
		["stone_bricks"] = new("stone_brick", "stone_brick", "stone_brick"),
		["mossy_stone_bricks"] = new("mossy_stone_brick", "mossy_stone_brick", "mossy_stone_brick"),
		["cobblestone"] = new("cobblestone", "cobblestone", "cobblestone"),
		["mossy_cobblestone"] = new("mossy_cobblestone", "mossy_cobblestone", "mossy_cobblestone"),
		["smooth_stone"] = new("smooth_stone", "smooth_stone", null),

		// Deepslate variants
		["deepslate_bricks"] = new("deepslate_brick", "deepslate_brick", "deepslate_brick"),
		["deepslate_tiles"] = new("deepslate_tile", "deepslate_tile", "deepslate_tile"),
		["cobbled_deepslate"] = new("cobbled_deepslate", "cobbled_deepslate", "cobbled_deepslate"),
		["polished_deepslate"] = new("polished_deepslate", "polished_deepslate", "polished_deepslate"),

		// Brick variants
		["bricks"] = new("brick", "brick", "brick"),
		["mud_bricks"] = new("mud_brick", "mud_brick", "mud_brick"),
		["nether_bricks"] = new("nether_brick", "nether_brick", "nether_brick"),
		["red_nether_bricks"] = new("red_nether_brick", "red_nether_brick", "red_nether_brick"),

		// Sandstone variants
		["sandstone"] = new("sandstone", "sandstone", "sandstone"),
		["smooth_sandstone"] = new("smooth_sandstone", "smooth_sandstone", null),
		["cut_sandstone"] = new(null, "cut_sandstone", null),
		["red_sandstone"] = new("red_sandstone", "red_sandstone", "red_sandstone"),
		["smooth_red_sandstone"] = new("smooth_red_sandstone", "smooth_red_sandstone", null),
		["cut_red_sandstone"] = new(null, "cut_red_sandstone", null),

		// Quartz variants
		["quartz_block"] = new("quartz", "quartz", null),
		["smooth_quartz"] = new("smooth_quartz", "smooth_quartz", null),

		// Prismarine variants
		["prismarine"] = new("prismarine", "prismarine", "prismarine"),
		["prismarine_bricks"] = new("prismarine_brick", "prismarine_brick", null),
		["dark_prismarine"] = new("dark_prismarine", "dark_prismarine", null),

		// Purpur
		["purpur_block"] = new("purpur", "purpur", null),

		// Blackstone variants
		["blackstone"] = new("blackstone", "blackstone", "blackstone"),
		["polished_blackstone"] = new("polished_blackstone", "polished_blackstone", "polished_blackstone"),
		["polished_blackstone_bricks"] = new("polished_blackstone_brick", "polished_blackstone_brick", "polished_blackstone_brick"),

		// Tuff variants
		["tuff"] = new("tuff", "tuff", "tuff"),
		["tuff_bricks"] = new("tuff_brick", "tuff_brick", "tuff_brick"),
		["polished_tuff"] = new("polished_tuff", "polished_tuff", "polished_tuff"),

		// Copper variants
		["cut_copper"] = new("cut_copper", "cut_copper", null),
		["exposed_cut_copper"] = new("exposed_cut_copper", "exposed_cut_copper", null),
		["weathered_cut_copper"] = new("weathered_cut_copper", "weathered_cut_copper", null),
		["oxidized_cut_copper"] = new("oxidized_cut_copper", "oxidized_cut_copper", null),
		["waxed_cut_copper"] = new("waxed_cut_copper", "waxed_cut_copper", null),
		["waxed_exposed_cut_copper"] = new("waxed_exposed_cut_copper", "waxed_exposed_cut_copper", null),
		["waxed_weathered_cut_copper"] = new("waxed_weathered_cut_copper", "waxed_weathered_cut_copper", null),
		["waxed_oxidized_cut_copper"] = new("waxed_oxidized_cut_copper", "waxed_oxidized_cut_copper", null),

		// Wood planks -> stairs/slabs use wood type prefix, not "planks"
		["oak_planks"] = new("oak", "oak", null),
		["spruce_planks"] = new("spruce", "spruce", null),
		["birch_planks"] = new("birch", "birch", null),
		["jungle_planks"] = new("jungle", "jungle", null),
		["acacia_planks"] = new("acacia", "acacia", null),
		["dark_oak_planks"] = new("dark_oak", "dark_oak", null),
		["mangrove_planks"] = new("mangrove", "mangrove", null),
		["cherry_planks"] = new("cherry", "cherry", null),
		["bamboo_planks"] = new("bamboo", "bamboo", null),
		["bamboo_mosaic"] = new("bamboo_mosaic", "bamboo_mosaic", null),
		["crimson_planks"] = new("crimson", "crimson", null),
		["warped_planks"] = new("warped", "warped", null),

		// Andesite/Granite/Diorite
		["andesite"] = new("andesite", "andesite", "andesite"),
		["polished_andesite"] = new("polished_andesite", "polished_andesite", null),
		["granite"] = new("granite", "granite", "granite"),
		["polished_granite"] = new("polished_granite", "polished_granite", null),
		["diorite"] = new("diorite", "diorite", "diorite"),
		["polished_diorite"] = new("polished_diorite", "polished_diorite", null),

		// End stone
		["end_stone_bricks"] = new("end_stone_brick", "end_stone_brick", "end_stone_brick"),
	};

	// Known fence families: base name -> (fence name, fence_gate name)
	// Wood types have fences/fence_gates, stone types generally only have walls.
	private static readonly Dictionary<string, (string fence, string fenceGate)> KnownFences = new()
	{
		["oak_planks"] = ("oak_fence", "oak_fence_gate"),
		["spruce_planks"] = ("spruce_fence", "spruce_fence_gate"),
		["birch_planks"] = ("birch_fence", "birch_fence_gate"),
		["jungle_planks"] = ("jungle_fence", "jungle_fence_gate"),
		["acacia_planks"] = ("acacia_fence", "acacia_fence_gate"),
		["dark_oak_planks"] = ("dark_oak_fence", "dark_oak_fence_gate"),
		["mangrove_planks"] = ("mangrove_fence", "mangrove_fence_gate"),
		["cherry_planks"] = ("cherry_fence", "cherry_fence_gate"),
		["bamboo_planks"] = ("bamboo_fence", "bamboo_fence_gate"),
		["crimson_planks"] = ("crimson_fence", "crimson_fence_gate"),
		["warped_planks"] = ("warped_fence", "warped_fence_gate"),
		["nether_bricks"] = ("nether_brick_fence", "nether_brick_fence"),
	};

	// Known pane equivalents
	private static readonly Dictionary<string, string> KnownPanes = new()
	{
		["glass"] = "glass_pane",
		["white_stained_glass"] = "white_stained_glass_pane",
		["black_stained_glass"] = "black_stained_glass_pane",
		["gray_stained_glass"] = "gray_stained_glass_pane",
		["light_gray_stained_glass"] = "light_gray_stained_glass_pane",
		["brown_stained_glass"] = "brown_stained_glass_pane",
		["red_stained_glass"] = "red_stained_glass_pane",
		["orange_stained_glass"] = "orange_stained_glass_pane",
		["yellow_stained_glass"] = "yellow_stained_glass_pane",
		["lime_stained_glass"] = "lime_stained_glass_pane",
		["green_stained_glass"] = "green_stained_glass_pane",
		["cyan_stained_glass"] = "cyan_stained_glass_pane",
		["light_blue_stained_glass"] = "light_blue_stained_glass_pane",
		["blue_stained_glass"] = "blue_stained_glass_pane",
		["purple_stained_glass"] = "purple_stained_glass_pane",
		["magenta_stained_glass"] = "magenta_stained_glass_pane",
		["pink_stained_glass"] = "pink_stained_glass_pane",
		["iron_bars"] = "iron_bars",
	};

	public static PaletteBlockMapping FromBaseName(string baseName)
	{
		var ns = GetNamespace(baseName);
		var bare = GetBareName(baseName);

		string? stairName = null, slabName = null, wallName = null;
		string? fenceName = null, fenceGateName = null, paneName = null;

		if (KnownFamilies.TryGetValue(bare, out var family))
		{
			stairName = family.StairPrefix != null ? $"{family.StairPrefix}_stairs" : null;
			slabName = family.SlabPrefix != null ? $"{family.SlabPrefix}_slab" : null;
			wallName = family.WallPrefix != null ? $"{family.WallPrefix}_wall" : null;
		}

		if (KnownFences.TryGetValue(bare, out var fences))
		{
			fenceName = fences.fence;
			fenceGateName = fences.fenceGate;
		}

		if (KnownPanes.TryGetValue(bare, out var pane))
		{
			paneName = pane;
		}

		// Fallback: use {base}_{suffix} for anything not in the lookup
		stairName ??= $"{bare}_stairs";
		slabName ??= $"{bare}_slab";
		wallName ??= $"{bare}_wall";
		fenceName ??= $"{bare}_fence";
		fenceGateName ??= $"{bare}_fence_gate";
		paneName ??= $"{bare}_pane";

		return new PaletteBlockMapping(
			Block: $"{ns}{bare}",
			Stairs: $"{ns}{stairName}",
			Slab: $"{ns}{slabName}",
			Wall: $"{ns}{wallName}",
			Fence: $"{ns}{fenceName}",
			FenceGate: $"{ns}{fenceGateName}",
			Pane: $"{ns}{paneName}"
		);
	}

	public string GetVariant(string variantType) => variantType switch
	{
		"block" => Block,
		"stairs" => Stairs,
		"slab" => Slab,
		"wall" => Wall,
		"fence" => Fence,
		"fence_gate" => FenceGate,
		"pane" => Pane,
		_ => throw new ArgumentException($"Unknown palette variant type: {variantType}")
	};

	private static string GetBareName(string name)
	{
		var colonIndex = name.IndexOf(':');
		return colonIndex >= 0 ? name[(colonIndex + 1)..] : name;
	}

	private static string GetNamespace(string name)
	{
		var colonIndex = name.IndexOf(':');
		return colonIndex >= 0 ? name[..(colonIndex + 1)] : "minecraft:";
	}

	private record BlockFamilyNames(string? StairPrefix, string? SlabPrefix, string? WallPrefix);
}
