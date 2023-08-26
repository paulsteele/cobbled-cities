using System.Text.Json;
using Minecraft.City.Datapack.Generator.Content.PackMetadata;

namespace Minecraft.City.Datapack.Generator.Writers;

public class JsonWriter
{
	public void Serialize(IWriteableData data)
	{
		var path = $"output/{data.Path}";
		var file = $"{path}/{data.FileName}";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		var contents = JsonSerializer.Serialize((object) data);
		File.WriteAllText(file, contents);
		Console.WriteLine($"Wrote to {file}");
	}
}