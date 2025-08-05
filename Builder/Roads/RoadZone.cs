using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public record RoadZone(DirectoryInfo Directory, string Name, IlPoint Origin)
{
	public RoadZone? NextZone { get; set; }

	public FileInfo[] GetFiles() => Directory.GetFiles();
}
