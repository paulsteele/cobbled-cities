using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class DynamicBuilding
{
	private readonly string _name;
	private readonly BuildingSection[] _bottoms;
	private readonly BuildingSection[] _mids;
	private readonly BuildingSection[] _tops;
	private readonly JigsawTileType _jigsawTileType;

	private DynamicBuilding(string name, JigsawTileType jigsawTileType, BuildingSection[] bottoms, BuildingSection[] mids, BuildingSection[] tops)
	{
		_name = name;
		_bottoms = bottoms;
		_mids = mids;
		_tops = tops;
		_jigsawTileType = jigsawTileType;
	}

	private const string DynamicBuildingBottomName = "bottom";
	private const string DynamicBuildingMidName = "mid";
	private const string DynamicBuildingTopName = "top";

	public static IEnumerable<DynamicBuilding> GetAllDynamicBuildings(string zoneFolderPath)
	{
		var dynamicPath = Path.Combine(zoneFolderPath, "dynamic");
		var dynamicDir = new DirectoryInfo(dynamicPath);

		if (!dynamicDir.Exists)
		{
			return [];
		}

		var subDirectories = JigsawTileTypeExtensions.BuildingTypes
			.Select(tileType => (directory: new DirectoryInfo(Path.Combine(dynamicPath, tileType.GetBuildingTypeFolderName())), tileType))
			.ToLookup(d => d.directory.Exists);

		foreach (var missingDirectoryInfo in subDirectories[false])
		{
			Console.Error.WriteLine($"Could not find {missingDirectoryInfo.directory}.");
		}

		return subDirectories[true]
			.SelectMany(tuple => tuple.directory.GetDirectories().Select(d => tuple with { directory = d }))
			.Select(tuple => GetDynamicBuilding(tuple.directory, tuple.tileType))
			.OfType<DynamicBuilding>()
			.ToArray();
	}

	private static DynamicBuilding? GetDynamicBuilding(DirectoryInfo baseDirectory, JigsawTileType tileType)
	{
		var bottomDirectory = new DirectoryInfo(Path.Combine(baseDirectory.FullName, DynamicBuildingBottomName));
		var midDirectory = new DirectoryInfo(Path.Combine(baseDirectory.FullName, DynamicBuildingMidName));
		var topDirectory = new DirectoryInfo(Path.Combine(baseDirectory.FullName, DynamicBuildingTopName));

		if (!bottomDirectory.Exists || !midDirectory.Exists || !topDirectory.Exists)
		{
			return null;
		}

		const string searchPattern = "*.nbt";

		var bottomFiles = bottomDirectory.GetFiles(searchPattern);
		var midFiles = midDirectory.GetFiles(searchPattern);
		var topFiles = topDirectory.GetFiles(searchPattern);

		if (bottomFiles.Length == 0 || midFiles.Length == 0 || topFiles.Length == 0)
		{
			return null;
		}

		return new DynamicBuilding(
			baseDirectory.Name,
			tileType,
			GetBuildingSections(bottomFiles),
			GetBuildingSections(midFiles),
			GetBuildingSections(topFiles)
		);
	}

	private static BuildingSection[] GetBuildingSections(FileInfo[] files) =>
		files.Select(f => new BuildingSection(new NbtFile(f.FullName).RootTag)).ToArray();

	public IEnumerable<FloorSectionInfo> ConstructFloorSections(int minHeight, int maxHeight)
	{
		var maxStep = maxHeight - 2;
		var isLong = _jigsawTileType == JigsawTileType.BuildingLong;

		// Determine split info from the first bottom section (all sections share the same footprint)
		LongSplitInfo? splitInfo = null;
		if (isLong)
		{
			var probe = _bottoms[0].Clone();
			probe.RotateBuildingJigsaws(true);
			probe.UpdateJigsaws();
			(_, _, splitInfo) = probe.SplitLong($"{_name}-probe");
		}

		// Generate bottom sections — one per variant per height
		for (var b = 0; b < _bottoms.Length; b++)
		{
			for (var height = minHeight; height <= maxHeight; height++)
			{
				var step = height - 2;
				var bottom = _bottoms[b].Clone();
				bottom.RotateBuildingJigsaws(true);
				bottom.UpdateJigsaws();

				var fileName = $"{_name}-bottom-{b}-h{height}";

				if (isLong)
				{
					var extensionName = $"{fileName}-extension";
					(bottom, var extension, _) = bottom.SplitLong(extensionName);

					bottom.AddVerticalJigsaws(_name, FloorType.Bottom, step);

					extension.FillEmptySpace(extendHeight: false);
					extension.DebugPrint();
					extension.SaveNbt(extensionName);

					yield return new FloorSectionInfo
					{
						Name = extensionName,
						Source = _name,
						FloorType = FloorType.Bottom,
						Height = height,
						ChainStep = null,
						JigsawTileType = JigsawTileType.BuildingLongExtension,
						IsExtension = true
					};
				}
				else
				{
					bottom.AddVerticalJigsaws(_name, FloorType.Bottom, step);
				}

				bottom.FillEmptySpace(extendHeight: false);
				bottom.DebugPrint();
				bottom.SaveNbt(fileName);

				yield return new FloorSectionInfo
				{
					Name = fileName,
					Source = _name,
					FloorType = FloorType.Bottom,
					Height = height,
					ChainStep = null,
					JigsawTileType = _jigsawTileType,
					IsExtension = false
				};
			}
		}

		// Generate mid sections — one per variant per chain step
		for (var m = 0; m < _mids.Length; m++)
		{
			for (var step = 1; step <= maxStep; step++)
			{
				var mid = _mids[m].Clone();
				var fileName = $"{_name}-mid-{m}-step-{step}";

				if (isLong && splitInfo != null)
				{
					var extensionName = $"{fileName}-extension";
					(mid, var extension) = mid.SplitLongFloor(splitInfo, extensionName);

					mid.AddVerticalJigsaws(_name, FloorType.Mid, step);

					extension.FillEmptySpace(extendHeight: false);
					extension.DebugPrint();
					extension.SaveNbt(extensionName);

					yield return new FloorSectionInfo
					{
						Name = extensionName,
						Source = _name,
						FloorType = FloorType.Mid,
						Height = null,
						ChainStep = step,
						JigsawTileType = JigsawTileType.BuildingLongExtension,
						IsExtension = true
					};
				}
				else
				{
					mid.AddVerticalJigsaws(_name, FloorType.Mid, step);
				}

				mid.FillEmptySpace(extendHeight: false);
				mid.DebugPrint();
				mid.SaveNbt(fileName);

				yield return new FloorSectionInfo
				{
					Name = fileName,
					Source = _name,
					FloorType = FloorType.Mid,
					Height = null,
					ChainStep = step,
					JigsawTileType = _jigsawTileType,
					IsExtension = false
				};
			}
		}

		// Generate top sections — one per variant (shared across all heights)
		for (var t = 0; t < _tops.Length; t++)
		{
			var top = _tops[t].Clone();
			var fileName = $"{_name}-top-{t}";

			if (isLong && splitInfo != null)
			{
				var extensionName = $"{fileName}-extension";
				(top, var extension) = top.SplitLongFloor(splitInfo, extensionName);

				top.AddVerticalJigsaws(_name, FloorType.Top, null);

				extension.FillEmptySpace(extendHeight: true);
				extension.DebugPrint();
				extension.SaveNbt(extensionName);

				yield return new FloorSectionInfo
				{
					Name = extensionName,
					Source = _name,
					FloorType = FloorType.Top,
					Height = null,
					ChainStep = null,
					JigsawTileType = JigsawTileType.BuildingLongExtension,
					IsExtension = true
				};
			}
			else
			{
				top.AddVerticalJigsaws(_name, FloorType.Top, null);
			}

			top.FillEmptySpace(extendHeight: true);
			top.DebugPrint();
			top.SaveNbt(fileName);

			yield return new FloorSectionInfo
			{
				Name = fileName,
				Source = _name,
				FloorType = FloorType.Top,
				Height = null,
				ChainStep = null,
				JigsawTileType = _jigsawTileType,
				IsExtension = false
			};
		}
	}
}
