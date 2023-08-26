using System.Text.Json.Serialization;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Content.Structure;

public class Structure : IWriteableData
{
	public Structure(string path, string fileName, string startPool)
	{
		Path = path;
		FileName = fileName;
		StartPool = startPool;
	}

	[JsonIgnore] public string Path { get; }
	[JsonIgnore] public string FileName { get; }

	[JsonPropertyName("type")] public string Type => "minecraft:jigsaw";
	[JsonPropertyName("biomes")] public string Biomes => "#minecraft:has_structure/mineshaft";
	[JsonPropertyName("step")] public string Step => "surface_structures";
	[JsonPropertyName("start_pool")] public string StartPool { get; }
	[JsonPropertyName("spawn_overrides")] public object SpawnOverrides => new object();
	[JsonPropertyName("terrain_adaptation")] public string TerrainAdaptation => "beard_thin";
	[JsonPropertyName("size")] public int Size => 1;
	[JsonPropertyName("start_height")] public StructureStartHeight StructureStartHeight => new(0);
	[JsonPropertyName("project_start_to_heightmap")] public string ProjectStartToHeightMap => "WORLD_SURFACE_WG";
	[JsonPropertyName("max_distance_from_center")] public int MaxDistanceFromCenter => 80;
	[JsonPropertyName("use_expansion_hack")] public bool UseExpansionHack => false;
}