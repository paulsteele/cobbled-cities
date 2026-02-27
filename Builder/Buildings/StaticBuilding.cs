using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public static class StaticBuilding
{
	public static IEnumerable<BuildingInfo> GetAllStaticBuildings(string zoneFolderPath)
	{
		var staticPath = Path.Combine(zoneFolderPath, "static");
		var staticDir = new DirectoryInfo(staticPath);

		if (!staticDir.Exists)
		{
			return [];
		}

		return JigsawTileTypeExtensions.BuildingTypes
			.SelectMany(tileType => GetStaticBuildingsForType(staticDir, tileType))
			.ToArray();
	}

	private static IEnumerable<BuildingInfo> GetStaticBuildingsForType(DirectoryInfo staticDir, JigsawTileType tileType)
	{
		var typeDir = new DirectoryInfo(Path.Combine(staticDir.FullName, tileType.GetBuildingTypeFolderName()));

		if (!typeDir.Exists)
		{
			return [];
		}

		return typeDir.GetFiles("*.nbt")
			.SelectMany(file => ProcessStaticBuilding(file, tileType))
			.ToArray();
	}

	private static IEnumerable<BuildingInfo> ProcessStaticBuilding(FileInfo file, JigsawTileType tileType)
	{
		var nbt = new NbtFile(file.FullName);
		var section = new BuildingSection(nbt.RootTag);

		section.RotateBuildingJigsaws(true);
		section.UpdateJigsaws();

		var fileName = Path.GetFileNameWithoutExtension(file.Name);

		if (tileType == JigsawTileType.BuildingLong)
		{
			var extensionName = $"{fileName}-extension";
			(section, var extension) = section.SplitLong(extensionName);

			extension.FillEmptySpace();
			extension.DebugPrint();
			extension.SaveNbt(extensionName);

			yield return new BuildingInfo
			{
				Name = extensionName,
				Source = fileName,
				Height = 0,
				JigsawTileType = JigsawTileType.BuildingLongExtension
			};
		}

		section.FillEmptySpace();
		section.DebugPrint();
		section.SaveNbt(fileName);

		yield return new BuildingInfo
		{
			Name = fileName,
			Source = fileName,
			Height = 0,
			JigsawTileType = tileType
		};
	}
}
