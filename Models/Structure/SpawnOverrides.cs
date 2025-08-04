using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Models.Structure;

public class SpawnOverrides
{
	[JsonPropertyName("monster")]
	public SpawnCategory Monster => new();
	
	[JsonPropertyName("creature")]
	public SpawnCategory Creature => new();
	
	[JsonPropertyName("ambient")]
	public SpawnCategory Ambient => new();
	
	[JsonPropertyName("axolotls")]
	public SpawnCategory Axolotls => new();
	
	[JsonPropertyName("underground_water_creature")]
	public SpawnCategory UndergroundWaterCreature => new();
	
	[JsonPropertyName("water_creature")]
	public SpawnCategory WaterCreature => new();
	
	[JsonPropertyName("water_ambient")]
	public SpawnCategory WaterAmbient => new();
	
	[JsonPropertyName("misc")]
	public SpawnCategory Misc => new();
}

public class SpawnCategory
{
	[JsonPropertyName("bounding_box")]
	public string BoundingBox => "full";
	
	[JsonPropertyName("spawns")]
	public object[] Spawns => [];
}