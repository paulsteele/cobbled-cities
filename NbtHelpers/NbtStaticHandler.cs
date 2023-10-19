using fNbt;
using Minecraft.City.Datapack.Generator.Tiling.Buildings;

namespace Minecraft.City.Datapack.Generator.NbtHelpers;

public class NbtStaticHandler
{
	private NbtFileFixer _fileFixer;

	public NbtStaticHandler(NbtFileFixer fileFixer)
	{
		_fileFixer = fileFixer;
	}

	public void CopyAndFixStaticFiles()
	{
		var roads = new DirectoryInfo("../../../nbts/roads");
		var buildings = new DirectoryInfo("../../../nbts/buildings");

		var roadFiles = roads.GetFiles("*.*", SearchOption.AllDirectories);
		var buildingFiles = buildings.GetFiles("*.*", SearchOption.AllDirectories);

		var staticFiles = roadFiles.Concat(buildingFiles);

		foreach (var file in staticFiles)
		{
			var directoryName = file.Directory.Name;
			var fileName = file.Name;

			var destinationDirectory = $"output/data/poke-cities/structures/{directoryName}";

			if (!Directory.Exists(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}

			var nbt = new NbtFile(file.ToString());	
			
			_fileFixer.FixFile(nbt);
			
			var destination = $"{destinationDirectory}/{fileName}";
			nbt.SaveToFile(destination, NbtCompression.GZip);
			
			Console.WriteLine($"Saved {nbt.FileName}");
		}
	}
}