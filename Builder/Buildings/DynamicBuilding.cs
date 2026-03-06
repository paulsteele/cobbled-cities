using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Buildings.Palette;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class DynamicBuilding
{
	private readonly string _name;
	private readonly BuildingSection[] _bottoms;
	private readonly BuildingSection[] _mids;
	private readonly BuildingSection[] _tops;
	private readonly JigsawTileType _jigsawTileType;
	private readonly PaletteConfig? _paletteConfig;

	private DynamicBuilding(string name, JigsawTileType jigsawTileType, BuildingSection[] bottoms, BuildingSection[] mids, BuildingSection[] tops, PaletteConfig? paletteConfig)
	{
		_name = name;
		_bottoms = bottoms;
		_mids = mids;
		_tops = tops;
		_jigsawTileType = jigsawTileType;
		_paletteConfig = paletteConfig;
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

		var paletteConfig = PaletteConfig.LoadFromDirectory(baseDirectory.FullName);

		return new DynamicBuilding(
			baseDirectory.Name,
			tileType,
			GetBuildingSections(bottomFiles),
			GetBuildingSections(midFiles),
			GetBuildingSections(topFiles),
			paletteConfig
		);
	}

	private static BuildingSection[] GetBuildingSections(FileInfo[] files) =>
		files.Select(f => new BuildingSection(new NbtFile(f.FullName).RootTag)).ToArray();

	private IReadOnlyList<PaletteCombination?> GetCombinations() =>
		_paletteConfig?.GetAllCombinations().Cast<PaletteCombination?>().ToList()
		?? [null];

	private string GetSource(PaletteCombination? combination) =>
		combination != null ? $"{_name}-{combination.Suffix}" : _name;

	private string GetFileName(string baseFileName, PaletteCombination? combination) =>
		combination != null ? $"{baseFileName}-{combination.Suffix}" : baseFileName;

	private static void ApplyPaletteAndSave(BuildingSection section, string fileName, PaletteCombination? combination, bool extendHeight)
	{
		if (combination != null)
		{
			PaletteReplacer.Apply(section.RootTag, combination);
		}

		section.FillEmptySpace(extendHeight: extendHeight);
		section.DebugPrint();
		section.SaveNbt(fileName);
	}

	public IEnumerable<FloorSectionInfo> ConstructFloorSections(int minHeight, int maxHeight)
	{
		var maxStep = maxHeight - 2;
		var isLong = _jigsawTileType == JigsawTileType.BuildingLong;
		var combinations = GetCombinations();

		// Determine split info from the first bottom section (all sections share the same footprint)
		LongSplitInfo? splitInfo = null;
		if (isLong)
		{
			var probe = _bottoms[0].Clone();
			probe.RotateBuildingJigsaws(true);
			probe.UpdateJigsaws();
			(_, _, splitInfo) = probe.SplitLong($"{_name}-probe");
		}

		// Generate bottom sections — one per variant per height per palette combination
		for (var b = 0; b < _bottoms.Length; b++)
		{
			for (var height = minHeight; height <= maxHeight; height++)
			{
				var step = height - 2;
				var baseFileName = $"{_name}-bottom-{b}-h{height}";

				foreach (var combination in combinations)
				{
					var source = GetSource(combination);
					var fileName = GetFileName(baseFileName, combination);
					var bottom = _bottoms[b].Clone();
					bottom.RotateBuildingJigsaws(true);
					bottom.UpdateJigsaws();

					if (isLong)
					{
						var extensionName = $"{fileName}-extension";
						(bottom, var extension, _) = bottom.SplitLong(extensionName);

						bottom.AddVerticalJigsaws(source, FloorType.Bottom, step);

						ApplyPaletteAndSave(extension, extensionName, combination, extendHeight: false);

						yield return new FloorSectionInfo
						{
							Name = extensionName,
							Source = source,
							FloorType = FloorType.Bottom,
							Height = height,
							ChainStep = null,
							JigsawTileType = JigsawTileType.BuildingLongExtension,
							IsExtension = true
						};
					}
					else
					{
						bottom.AddVerticalJigsaws(source, FloorType.Bottom, step);
					}

					ApplyPaletteAndSave(bottom, fileName, combination, extendHeight: false);

					yield return new FloorSectionInfo
					{
						Name = fileName,
						Source = source,
						FloorType = FloorType.Bottom,
						Height = height,
						ChainStep = null,
						JigsawTileType = _jigsawTileType,
						IsExtension = false
					};
				}
			}
		}

		// Generate mid sections — one per variant per chain step per palette combination
		for (var m = 0; m < _mids.Length; m++)
		{
			for (var step = 1; step <= maxStep; step++)
			{
				var baseFileName = $"{_name}-mid-{m}-step-{step}";

				foreach (var combination in combinations)
				{
					var source = GetSource(combination);
					var fileName = GetFileName(baseFileName, combination);
					var mid = _mids[m].Clone();

					if (isLong && splitInfo != null)
					{
						var extensionName = $"{fileName}-extension";
						(mid, var extension) = mid.SplitLongFloor(splitInfo, extensionName);

						mid.AddVerticalJigsaws(source, FloorType.Mid, step);

						ApplyPaletteAndSave(extension, extensionName, combination, extendHeight: false);

						yield return new FloorSectionInfo
						{
							Name = extensionName,
							Source = source,
							FloorType = FloorType.Mid,
							Height = null,
							ChainStep = step,
							JigsawTileType = JigsawTileType.BuildingLongExtension,
							IsExtension = true
						};
					}
					else
					{
						mid.AddVerticalJigsaws(source, FloorType.Mid, step);
					}

					ApplyPaletteAndSave(mid, fileName, combination, extendHeight: false);

					yield return new FloorSectionInfo
					{
						Name = fileName,
						Source = source,
						FloorType = FloorType.Mid,
						Height = null,
						ChainStep = step,
						JigsawTileType = _jigsawTileType,
						IsExtension = false
					};
				}
			}
		}

		// Generate top sections — one per variant per palette combination (shared across all heights)
		for (var t = 0; t < _tops.Length; t++)
		{
			var baseFileName = $"{_name}-top-{t}";

			foreach (var combination in combinations)
			{
				var source = GetSource(combination);
				var fileName = GetFileName(baseFileName, combination);
				var top = _tops[t].Clone();

				if (isLong && splitInfo != null)
				{
					var extensionName = $"{fileName}-extension";
					(top, var extension) = top.SplitLongFloor(splitInfo, extensionName);

					top.AddVerticalJigsaws(source, FloorType.Top, null);

					ApplyPaletteAndSave(extension, extensionName, combination, extendHeight: true);

					yield return new FloorSectionInfo
					{
						Name = extensionName,
						Source = source,
						FloorType = FloorType.Top,
						Height = null,
						ChainStep = null,
						JigsawTileType = JigsawTileType.BuildingLongExtension,
						IsExtension = true
					};
				}
				else
				{
					top.AddVerticalJigsaws(source, FloorType.Top, null);
				}

				ApplyPaletteAndSave(top, fileName, combination, extendHeight: true);

				yield return new FloorSectionInfo
				{
					Name = fileName,
					Source = source,
					FloorType = FloorType.Top,
					Height = null,
					ChainStep = null,
					JigsawTileType = _jigsawTileType,
					IsExtension = false
				};
			}
		}
	}
}
