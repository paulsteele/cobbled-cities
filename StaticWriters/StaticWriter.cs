namespace Minecraft.City.Datapack.Generator.StaticWriters;

public abstract class StaticWriter : IStaticWriter
{
	protected abstract string Path { get; }
	protected abstract string FileName { get; }
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

public interface IStaticWriter
{
	public void Serialize();
}