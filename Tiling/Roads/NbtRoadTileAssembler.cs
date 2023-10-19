using fNbt;
using Minecraft.City.Datapack.Generator.NbtHelpers;

namespace Minecraft.City.Datapack.Generator.Tiling.Roads;

// ReSharper disable once ClassNeverInstantiated.Global
public class NbtRoadTileAssembler
{
	public void CreatePortions()
	{
		var centers = new DirectoryInfo("../../../nbts/centers");
		
		CreateType(centers);
	}

	private void CreateType(DirectoryInfo directory)
	{
		var files = directory.GetFiles();

		foreach (var file in files)
		{
			DeconstructFile(file);
		}
	}

	private void DeconstructFile(FileSystemInfo fileInfo)
	{
		var nbt = new NbtFile(fileInfo.FullName);
		Console.WriteLine(nbt.FileName);

		var grid = ConvertNbtToGrid(nbt);
		
		grid.DebugPrint();
	}

	private RoadSection ConvertNbtToGrid(NbtFile nbtFile)
	{
		var blocks = nbtFile.RootTag.Get<NbtList>("blocks");
		
		if (blocks == null)
		{
			return new RoadSection();
		}

		var (x, _, z) = nbtFile.RootTag.GetNbtDimensions();

		var grid = new RoadTile[x, z];

		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var (posX, _,  posZ) = compound.GetNbtPosition();
			
			grid[posX, posZ] = RoadTile.Filled;

			if (!compound.IsJigsaw())
			{
				continue;
			}

			var orientation = compound.GetPaletteTag("orientation");

			grid[posX, posZ] = orientation switch
			{
				"north_up" => RoadTile.North,
				"south_up" => RoadTile.South,
				"west_up" => RoadTile.West,
				"east_up" => RoadTile.East,
				_ => grid[posX, posZ]
			};
		}

		return new RoadSection {Grid = grid};
	}
}