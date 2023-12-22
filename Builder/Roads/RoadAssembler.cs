using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

// ReSharper disable once ClassNeverInstantiated.Global
public class RoadAssembler : IAssembler
{
	public void Assemble()
	{
		var centers = new DirectoryInfo("../../../nbts/centers");
		
		CreateType(centers);
	}

	private void CreateType(DirectoryInfo directory)
	{
		var files = directory.GetFiles();

		foreach (var file in files)
		{
			DeconstructFile(file);
		}
	}

	private void DeconstructFile(FileSystemInfo fileInfo)
	{
		var nbt = new NbtFile(fileInfo.FullName);

		var road = new RoadSection(nbt.RootTag);
		
		var subSections = new List<RoadSection>();
		
		while (road.HasSubSections)
		{
			subSections.Add(road.TakeSubSection());
		}

		var subSectionDictionary = subSections
			.SelectMany(s => s.Jigsaws.Values.Select(k => (k.OriginalLocation, s.Index)))
			.ToDictionary();

		foreach (var subSection in subSections)
		{
			subSection.UpdateJigsaws(Path.GetFileNameWithoutExtension(fileInfo.Name), subSectionDictionary);
		}
		
		Console.WriteLine();
		//serialize
	}
}