using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public class RoadTile
{
	public RoadTileType Type { get; set; }
	public IlPoint Location { get; set; }
}