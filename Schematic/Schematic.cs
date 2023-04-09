using fNbt;

namespace schematic_to_lost_cities.Schematic;

public class Schematic
{
	public readonly NbtCompound[,,] Blocks;
	public readonly NbtCompound[] Palette;
	
	public Schematic(string filePath)
	{
		var file = new NbtFile(filePath);
		var size = file.RootTag.Get<NbtList>("size");
		
		var x = size.Get<NbtInt>(0).IntValue;
		var y = size.Get<NbtInt>(1).IntValue;
		var z = size.Get<NbtInt>(2).IntValue;

		Blocks = new NbtCompound[y, x, z];
		Palette = file.RootTag.Get<NbtList>("palette").ToArray<NbtCompound>();
		
		var blocks = file.RootTag.Get<NbtList>("blocks");

		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}
			
			var pos = compound.Get<NbtList>("pos");
			var blockX = pos.Get<NbtInt>(0).IntValue;
			var blockY = pos.Get<NbtInt>(1).IntValue;
			var blockZ = pos.Get<NbtInt>(2).IntValue;

			var state = compound.Get<NbtInt>("state").IntValue;

			Blocks[blockY, blockX, blockZ] = Palette[state];
		}
	}
}