using System.Text.Json;
using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings.Palette;

public record PaletteConfig(
	[property: JsonPropertyName("palettes")]
	PaletteAEntry[] Palettes
)
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public static PaletteConfig? LoadFromDirectory(string directoryPath)
	{
		var path = Path.Combine(directoryPath, "palettes.json");
		if (!File.Exists(path))
		{
			return null;
		}

		var json = File.ReadAllText(path);
		return JsonSerializer.Deserialize<PaletteConfig>(json, JsonOptions);
	}

	public List<PaletteCombination> GetAllCombinations()
	{
		var combinations = new List<PaletteCombination>();

		foreach (var aEntry in Palettes)
		{
			if (aEntry.B == null || aEntry.B.Length == 0)
			{
				combinations.Add(new PaletteCombination(aEntry.A, null, null, null));
				continue;
			}

			foreach (var bEntry in aEntry.B)
			{
				if (bEntry.C == null || bEntry.C.Length == 0)
				{
					combinations.Add(new PaletteCombination(aEntry.A, bEntry.Block, null, null));
					continue;
				}

				foreach (var cEntry in bEntry.C)
				{
					if (cEntry.D == null || cEntry.D.Length == 0)
					{
						combinations.Add(new PaletteCombination(aEntry.A, cEntry.Block, null, null));
						continue;
					}

					foreach (var dEntry in cEntry.D)
					{
						combinations.Add(new PaletteCombination(aEntry.A, bEntry.Block, cEntry.Block, dEntry.Block));
					}
				}
			}
		}

		return combinations;
	}
}

public record PaletteAEntry(
	[property: JsonPropertyName("a")]
	string A,
	[property: JsonPropertyName("b")]
	PaletteBEntry[]? B
);

public record PaletteBEntry(
	[property: JsonPropertyName("block")]
	string Block,
	[property: JsonPropertyName("c")]
	PaletteCEntry[]? C
);

public record PaletteCEntry(
	[property: JsonPropertyName("block")]
	string Block,
	[property: JsonPropertyName("d")]
	PaletteDEntry[]? D
);

public record PaletteDEntry(
	[property: JsonPropertyName("block")]
	string Block
);

public record PaletteCombination(string A, string? B, string? C, string? D)
{
	public string Suffix => string.Join("-",
		new[] { A, B, C, D }.Where(s => s != null));

	public PaletteBlockMapping? GetMappingForFamily(char family) => family switch
	{
		'a' => PaletteBlockMapping.FromBaseName(A),
		'b' => B != null ? PaletteBlockMapping.FromBaseName(B) : null,
		'c' => C != null ? PaletteBlockMapping.FromBaseName(C) : null,
		'd' => D != null ? PaletteBlockMapping.FromBaseName(D) : null,
		_ => throw new ArgumentException($"Unknown palette family: {family}")
	};
}
