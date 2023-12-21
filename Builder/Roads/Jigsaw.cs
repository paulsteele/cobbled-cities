using fNbt;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public class Jigsaw
{  
	public JigsawTileType TileType { get; }
	public IlPoint Location { get; }
	public IlPoint OriginalLocation { get; set; }
	public IlPoint? PointingToLocation { get; set; }
	public NbtCompound Compound { get; }

	public Jigsaw(NbtCompound compound, NbtCompound rootTag, IlPoint location)
	{
		Compound = compound;
		TileType = compound.GetJigsawTileType(rootTag);
		Location = location;
	}
}