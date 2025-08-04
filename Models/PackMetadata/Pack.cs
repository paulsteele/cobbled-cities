using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Models.PackMetadata;

public class Pack
{
	[JsonPropertyName("pack_format")] public int PackFormat => 48;
	[JsonPropertyName("description")] public string Description => "Poke Cities";
}