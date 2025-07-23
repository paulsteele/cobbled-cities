using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingSection(NbtCompound rootTag) : AbstractSection(rootTag)
{
	public string SaveNbt(string fileName)
	{
		var outputPath = $"output/data/poke-cities/structure/buildings";
		if (!Directory.Exists(outputPath))
		{
			Directory.CreateDirectory(outputPath);
		}

		var path = $"{outputPath}/{fileName}.nbt";

		var nbt = new NbtFile(RootTag);

		nbt.SaveToFile(path, NbtCompression.GZip);

		Console.WriteLine($"Saved {path}");

		return path;
	}

	public void UpdateJigsaws()
	{
		foreach (var jigsaw in Jigsaws.Values)
		{
			jigsaw.SetJigsawName("poke-cities:buildings-start");
			// jigsaw.SetJigsawPool($"poke-cities:buildings");
			// jigsaw.SetJigsawTarget($"poke-cities:buildings-start");
		}
	}
}