using fNbt;

namespace schematic_to_lost_cities.Schematic;

public class Schematic
{
	public readonly int[,,] Blocks;
	public string Name;
	public NbtCompound[] Palette { get; private set; }
	
	public Schematic(string filePath)
	{
		Name = filePath.Replace(".nbt", "");
		var file = new NbtFile(filePath);
		var size = file.RootTag.Get<NbtList>("size");
		
		var x = size.Get<NbtInt>(0).IntValue;
		var y = size.Get<NbtInt>(1).IntValue;
		var z = size.Get<NbtInt>(2).IntValue;

		Blocks = new int[y, x, z];
		ReplaceAllReferences(0, -1);
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

			Blocks[blockY, blockX, blockZ] = state;
		}
	}

	public void UpdateToUseConsolidatedPalette(ConsolidatedPalette consolidatedPalette)
	{
		var equalityChecker = new PaletteEquality();
		for (var i = 0; i < Palette.Length; i++)
		{
			for (var j = 0; j < consolidatedPalette.Palette.Length; j++)
			{
				if (equalityChecker.Equals(Palette[i], consolidatedPalette.Palette[j]))
				{
					var replaced= ReplaceAllReferences(i, j);
					Console.WriteLine($"{Name} - replaced {replaced} from id {i} to {j}");
				}
			}
		}

		Palette = consolidatedPalette.Palette;
	}

	private int ReplaceAllReferences(int oldVal, int newVal)
	{
		var replaces = 0;
		for (var y = 0; y < Blocks.GetLength(0); y++)
		{
			for (var x = 0; x < Blocks.GetLength(1); x++)
			{
				for (var z = 0; z < Blocks.GetLength(2); z++)
				{
					if (Blocks[y, x, z] == oldVal)
					{
						Blocks[y, x, z] = newVal;
						replaces++;
					}
				}
			}
		}

		return replaces;
	}
}