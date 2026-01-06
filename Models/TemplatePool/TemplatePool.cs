using System.Text.Json.Serialization;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Models.TemplatePool;

public class TemplatePool : IWriteableData
{
	public TemplatePool(string path, string name, IEnumerable<TemplatePoolElementWeight> elementWeights)
	{
		Path = path;
		FileName = name;
		Name = $"cobbled-cities:{name}";
		Elements = elementWeights.ToArray();
	}

	[JsonIgnore] public string Path { get; }
	[JsonIgnore] public string FileName { get; }
	
	[JsonPropertyName("name")] public string Name { get; }
	[JsonPropertyName("fallback")] public string Fallback => "minecraft:empty";
	[JsonPropertyName("elements")] public TemplatePoolElementWeight[] Elements { get; }

}
