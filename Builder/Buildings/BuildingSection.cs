using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingSection(NbtCompound rootTag) : AbstractSection(rootTag)
{
	public string SaveNbt(string fileName)
	{
		var outputPath = $"output/data/cobbled-cities/structure/buildings";
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
			jigsaw.SetJigsawName("cobbled-cities:buildings-start");
			// jigsaw.SetJigsawPool($"cobbled-cities:buildings");
			// jigsaw.SetJigsawTarget($"cobbled-cities:buildings-start");
		}
	}

	public BuildingSection Clone()
	{
		return new BuildingSection(RootTag);
	}
}
