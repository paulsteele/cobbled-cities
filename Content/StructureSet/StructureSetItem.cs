using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Content.StructureSet;

public class StructureSetItem
{
	// ReSharper disable once SuggestBaseTypeForParameterInConstructor
	public StructureSetItem(Structure.Structure structure, int weight)
	{
		Structure = $"poke-cities:{structure.FileName}";
		Weight = weight;
	}

	[JsonPropertyName("structure")] public string Structure { get; }
	[JsonPropertyName("weight")] public int Weight { get; }
}