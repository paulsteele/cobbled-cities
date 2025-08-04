using System.Text.Json.Serialization;

namespace Minecraft.City.Datapack.Generator.Models.Structure;

public class StructureStartHeight
{
	public StructureStartHeight(int absolute)
	{
		Absolute = absolute;
	}

	[JsonPropertyName("absolute")] public int Absolute { get; }
}