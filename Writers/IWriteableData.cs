using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Writers;

public interface IWriteableData
{
	[JsonIgnore]
	string Path { get; }
	
	[JsonIgnore]
	string FileName { get; }
}