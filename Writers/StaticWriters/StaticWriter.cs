namespace Minecraft.City.Datapack.Generator.Writers.StaticWriters;

public abstract class StaticWriter : IStaticWriter
{
	public abstract string Path { get; }
	public abstract string FileName { get; }
	protected abstract string Contents { get; }

	public void Serialize()
	{
		var path = $"output/{Path}";
		var file = $"{path}/{FileName}";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		File.WriteAllText(file, Contents);
		Console.WriteLine($"Wrote to {file}");
	}
}

public interface IStaticWriter : IWriteableData
{
	public void Serialize();
}