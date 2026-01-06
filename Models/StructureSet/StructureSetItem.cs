using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Models.StructureSet;

public class StructureSetItem
{
	// ReSharper disable once SuggestBaseTypeForParameterInConstructor
	public StructureSetItem(Models.Structure.Structure structure, int weight)
	{
		Structure = $"cobbled-cities:{structure.FileName}";
		Weight = weight;
	}

	[JsonPropertyName("structure")] public string Structure { get; }
	[JsonPropertyName("weight")] public int Weight { get; }
}
