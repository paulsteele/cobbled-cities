using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

// ReSharper disable once ClassNeverInstantiated.Global
public class RoadAssembler : IAssembler
{
	public void Assemble()
	{
		var centers = new DirectoryInfo("../../../nbts/centers");
		
		CreateType(centers, nameof(centers));
	}

	private void CreateType(DirectoryInfo directory, string typeName)
	{
		var files = directory.GetFiles();

		foreach (var file in files)
		{
			DeconstructFile(file, typeName);
		}
	}

	private void DeconstructFile(FileSystemInfo fileInfo, string typeName)
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
		
		var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

		foreach (var subSection in subSections)
		{
			subSection.UpdateJigsaws(fileName, subSectionDictionary);
			subSection.SaveNbt(fileName, typeName);
		}
		
		
		
	}
}