using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public record BuildingZone
{
	private string Name { get; }
	public int MinHeight { get; }
	public int MaxHeight { get; }
	public int MaxDistanceFromCenter { get; }

	public BuildingZone(string name, int minHeight, int maxHeight, int maxDistanceFromCenter)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Zone name cannot be null or empty", nameof(name));
		}

		if (minHeight <= 0)
		{
			throw new ArgumentException("Minimum height must be greater than 0", nameof(minHeight));
		}

		if (maxHeight < minHeight)
		{
			throw new ArgumentException($"Maximum height ({maxHeight}) must be greater than or equal to minimum height ({minHeight})", nameof(maxHeight));
		}

		if (maxDistanceFromCenter < 0)
		{
			throw new ArgumentException("Maximum distance from center cannot be negative", nameof(maxDistanceFromCenter));
		}

		Name = name;
		MinHeight = minHeight;
		MaxHeight = maxHeight;
		MaxDistanceFromCenter = maxDistanceFromCenter;
	}
	
	public string GetNameForType(JigsawTileType type) => $"{Name}-{type.GetBuildingTypeFolderName()}";
}
