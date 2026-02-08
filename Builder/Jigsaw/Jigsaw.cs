using fNbt;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Jigsaw;

public class Jigsaw
{  
	public JigsawTileType TileType { get; set; }
	public IlPoint Location { get; }
	public IlPoint OriginalLocation { get; set; }
	public IlPoint? PointingToLocation { get; set; }
	public NbtCompound Compound { get; }
	public bool PointsToOutside { get; set; }
	public bool PointsFromOutside { get; set; }

	public bool IsBuilding => TileType
		is JigsawTileType.BuildingNormal
		or JigsawTileType.BuildingCorner
		or JigsawTileType.BuildingLong;

	public Jigsaw(NbtCompound compound, NbtCompound rootTag, IlPoint location)
	{
		Compound = compound;
		TileType = compound.GetJigsawTileType(rootTag);
		Location = location;
	}

	public void Flip(Dictionary<JigsawTileType, int> states)
	{
		TileType = TileType.Rotated180DegreesTileType();
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

	public string GetJigsawName() => GetNbtField("name");
	public string GetJigsawPool() => GetNbtField("pool");
	public string GetJigsawTarget() => GetNbtField("target");

	private string GetNbtField(string fieldName)
	{
		var nbt = Compound.Get<NbtCompound>("nbt");
		return nbt?.Get<NbtString>(fieldName)?.Value ?? string.Empty;
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
