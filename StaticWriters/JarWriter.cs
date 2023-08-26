using System.IO.Compression;

namespace Minecraft.City.Datapack.Generator.StaticWriters;

public class JarWriter
{
	public void CreateJar()
	{
		const string jarFile = "poke-cities.jar";
		if (File.Exists(jarFile))
		{
			File.Delete(jarFile);
		}
		ZipFile.CreateFromDirectory("output", jarFile);
		Console.WriteLine($"Wrote {jarFile}");
	}
}