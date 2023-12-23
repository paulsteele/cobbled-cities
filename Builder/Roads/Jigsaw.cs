using fNbt;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public class Jigsaw
{  
	public JigsawTileType TileType { get; private set; }
	public IlPoint Location { get; }
	public IlPoint OriginalLocation { get; set; }
	public IlPoint? PointingToLocation { get; set; }
	public NbtCompound Compound { get; }
	public bool PointsToOutside { get; set; }

	public Jigsaw(NbtCompound compound, NbtCompound rootTag, IlPoint location)
	{
		Compound = compound;
		TileType = compound.GetJigsawTileType(rootTag);
		Location = location;
	}

	public void Flip(Dictionary<JigsawTileType, int> states)
	{
		TileType = TileType.FlippedTileType();
		Compound.SetState(states[TileType]);
	}

	public void SetJigsawName(string value)
	{
		SetNbtField("name", value);
	}
	
	public void SetJigsawPool(string value)
	{
		SetNbtField("pool", value);
	}
	
	public void SetJigsawTarget(string value)
	{
		SetNbtField("target", value);
	}

	private void SetNbtField(string fieldName, string value)
	{
		var nbt = Compound.Get<NbtCompound>("nbt");

		if (nbt == null)
		{
			return;
		}

		nbt[fieldName] = new NbtString(fieldName, value);

	}
}