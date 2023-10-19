using fNbt;

namespace Minecraft.City.Datapack.Generator.NbtHandler;

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

	private GridSection ConvertNbtToGrid(NbtFile nbtFile)
	{
		var blocks = nbtFile.RootTag.Get<NbtList>("blocks");
		
		if (blocks == null)
		{
			return new GridSection();
		}

		var (x, _, z) = nbtFile.RootTag.GetNbtDimensions();

		var grid = new GridSectionTile[x, z];

		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var (posX, _,  posZ) = compound.GetNbtPosition();
			
			grid[posX, posZ] = GridSectionTile.Filled;

			if (!compound.IsJigsaw())
			{
				continue;
			}

			var orientation = compound.GetPaletteTag("orientation");

			grid[posX, posZ] = orientation switch
			{
				"north_up" => GridSectionTile.North,
				"south_up" => GridSectionTile.South,
				"west_up" => GridSectionTile.West,
				"east_up" => GridSectionTile.East,
				_ => grid[posX, posZ]
			};
		}

		return new GridSection {Grid = grid};
	}
}