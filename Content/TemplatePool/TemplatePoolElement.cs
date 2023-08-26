using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Content.TemplatePool;

public class TemplatePoolElement
{
	public TemplatePoolElement(string location)
	{
		Location = location;
	}

	[JsonPropertyName("element_type")] public string ElementType => "minecraft:single_pool_element";
	[JsonPropertyName("location")] public string Location { get; }
	[JsonPropertyName("projection")] public string Projection => "rigid";
	[JsonPropertyName("processors")] public string Processors => "minecraft:empty";
}