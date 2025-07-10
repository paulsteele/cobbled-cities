using System.Text.Json.Serialization;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Models.Structure;

public class Structure : IWriteableData
{
	public Structure(string path, string fileName, TemplatePool.TemplatePool startPool)
	{
		Path = path;
		FileName = fileName;
		StartPool = startPool.Name;
	}

	[JsonIgnore] public string Path { get; }
	[JsonIgnore] public string FileName { get; }

	[JsonPropertyName("type")] public string Type => "minecraft:jigsaw";
	[JsonPropertyName("biomes")] public string Biomes => "#minecraft:is_overworld";
	[JsonPropertyName("step")] public string Step => "underground_structures";
	[JsonPropertyName("start_pool")] public string StartPool { get; }
	[JsonPropertyName("spawn_overrides")] public object SpawnOverrides => new object();
	[JsonPropertyName("terrain_adaptation")] public string TerrainAdaptation => "beard_thin";
	[JsonPropertyName("size")] public int Size => 7;
	[JsonPropertyName("start_height")] public StructureStartHeight StructureStartHeight => new(0);
	[JsonPropertyName("project_start_to_heightmap")] public string ProjectStartToHeightMap => "WORLD_SURFACE_WG";
	[JsonPropertyName("max_distance_from_center")] public int MaxDistanceFromCenter => 500; // update to 500 with jigsaw expander
	[JsonPropertyName("use_expansion_hack")] public bool UseExpansionHack => false;
}