using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Models.TemplatePool;

public class TemplatePoolElementWeight
{
	public TemplatePoolElementWeight(string location, int weight)
	{
		Element = new TemplatePoolElement(location);
		Weight = weight;
	}

	[JsonPropertyName("weight")] public int Weight { get; }
	[JsonPropertyName("element")] public TemplatePoolElement Element { get; }
}