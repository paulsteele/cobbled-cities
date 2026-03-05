using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public record BuildingInfo
{
	public required string Name { get; init; }
	public required string Source { get; init; }
	public required int Height { get; init; }
	public required JigsawTileType JigsawTileType { get; init; }
}

public enum FloorType { Bottom, Mid, Top }

public record FloorSectionInfo
{
	public required string Name { get; init; }
	public required string Source { get; init; }
	public required FloorType FloorType { get; init; }
	public required int? Height { get; init; }
	public required int? ChainStep { get; init; }
	public required JigsawTileType JigsawTileType { get; init; }
	public required bool IsExtension { get; init; }
}
