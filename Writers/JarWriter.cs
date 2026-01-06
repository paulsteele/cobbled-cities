using System.IO.Compression;

namespace Minecraft.City.Datapack.Generator.Writers;

public class JarWriter
{
	public void CreateJar()
	{
		const string jarFile = "cobbled-cities.jar";
		if (File.Exists(jarFile))
		{
			File.Delete(jarFile);
		}
		ZipFile.CreateFromDirectory("output", jarFile);
		Console.WriteLine($"Zipped output to {jarFile}");
	}
}
