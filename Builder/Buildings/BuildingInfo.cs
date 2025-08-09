using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public record BuildingInfo
{
	public required string Name { get; init; }
	public required int Height { get; init; }
	public required JigsawTileType JigsawTileType { get; init; }
}
