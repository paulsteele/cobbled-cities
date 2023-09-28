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
		
		PrintDebugNbt(nbt);
	}

	private void PrintDebugNbt(NbtFile nbtFile)
	{
		Console.WriteLine($"{nbtFile.FileName}");
		var blocks = nbtFile.RootTag.Get<NbtList>("blocks");

		var (x, _, z) = nbtFile.RootTag.GetNbtDimensions();

		var grid = new char[x, z];
		
		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var (posX, _,  posZ) = compound.GetNbtPosition();
			
			grid[posX, posZ] = 'x';

			if (!compound.IsJigsaw())
			{
				continue;
			}

			var orientation = compound.GetPaletteTag("orientation");

			grid[posX, posZ] = orientation switch
			{
				"north_up" => '↑',
				"south_up" => '↓',
				"west_up" => '←',
				"east_up" => '→',
				_ => grid[posX, posZ]
			};
		}

		for (var i = 0; i < grid.GetLength(0); i++)
		{
			for (var j = 0; j < grid.GetLength(1); j++)
			{
				Console.Write(grid[j, i]);
			}
			Console.WriteLine();
		}
	}
}