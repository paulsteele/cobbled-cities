using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Content.StructureSet;

public class StructureSetPlacement
{
	public StructureSetPlacement(int spacing, int separation)
	{
		Spacing = spacing;
		Separation = separation;
		Salt = new Random().Next(0, int.MaxValue);
	}

	[JsonPropertyName("type")] public string Type => "minecraft:random_spread";
	[JsonPropertyName("spacing")] public int Spacing { get; }
	[JsonPropertyName("separation")] public int Separation { get; }
	[JsonPropertyName("salt")] public int Salt { get; }
}