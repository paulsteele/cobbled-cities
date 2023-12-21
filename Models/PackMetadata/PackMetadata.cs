using System.Text.Json.Serialization;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Models.PackMetadata;

public class PackMetadata : IWriteableData
{
	[JsonPropertyName("pack")] public Pack Pack => new();
	[JsonIgnore] public string Path => "";
	[JsonIgnore] public string FileName => "pack";
}