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
		Console.WriteLine(nbt.FileName);

		var road = new RoadSection(nbt.RootTag);
		
		road.DebugPrint();

		var subSections = new List<RoadSection>();
		
		while (road.HasSubSections)
		{
			subSections.Add(road.TakeSubSection());
		}

		foreach (var subSection in subSections)
		{
			subSection.DebugPrint();
			subSection.FlipPointedToJigsaws();
			subSection.DebugPrint();
		}
		
		// * targeting right subsection
		
		//serialize
	}
}