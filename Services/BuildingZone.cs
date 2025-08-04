namespace Minecraft.City.Datapack.Generator.Services;

public record BuildingZone
{
	public string Name { get; }
	public int MinHeight { get; }
	public int MaxHeight { get; }

	public BuildingZone(string name, int minHeight, int maxHeight)
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

		Name = name;
		MinHeight = minHeight;
		MaxHeight = maxHeight;
	}
}