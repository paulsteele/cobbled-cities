using System.Text.Json;

namespace Minecraft.City.Datapack.Generator.Writers;

public class JsonWriter
{
	public void Serialize(IWriteableData data, string extension = ".json")
	{
		var path = $"output/{data.Path}";
		var file = $"{path}/{data.FileName}{extension}";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		var contents = JsonSerializer.Serialize((object) data, new JsonSerializerOptions{ WriteIndented = true});
		File.WriteAllText(file, contents);
		Console.WriteLine($"Wrote to {file}");
	}
}