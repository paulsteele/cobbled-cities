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
			return new RoadSection(0, 0);
		}

		var (x, _, z) = nbtFile.RootTag.GetNbtDimensions();

		var section = new RoadSection(x, z);

		foreach (var block in blocks)
		{
			if (block is not NbtCompound compound)
			{
				continue;
			}

			var (posX, _,  posZ) = compound.GetNbtPosition();

			var tile = new RoadTile
			{
				X = posX,
				Z = posZ
			};
			
			if (compound.IsJigsaw())
			{
				var orientation = compound.GetPaletteTag("orientation");

				tile.Type = orientation switch
				{
					"north_up" => RoadTileType.North,
					"south_up" => RoadTileType.South,
					"west_up" => RoadTileType.West,
					"east_up" => RoadTileType.East,
					_ => RoadTileType.Filled
				};
			}
			else
			{
				tile.Type = RoadTileType.Filled;
			}
			
			section.AddTile(tile);
		}

		return section;
	}
}