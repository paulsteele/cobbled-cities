using System.Text.Json.Serialization;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Content.StructureSet;

public class StructureSet : IWriteableData
{
	public StructureSet(
		string path, 
		string fileName,
		int spacing,
		int separation,
		IEnumerable<StructureSetItem> structureSets
	)
	{
		Path = path;
		FileName = fileName;

		Structures = structureSets.ToArray();
		Placement = new StructureSetPlacement(spacing, separation);
	}

	[JsonIgnore] public string Path { get; }
	[JsonIgnore] public string FileName { get; }

	[JsonPropertyName("structures")] public StructureSetItem[] Structures { get; }
	[JsonPropertyName("placement")] public StructureSetPlacement Placement { get; }

}