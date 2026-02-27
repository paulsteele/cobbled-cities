using Combinatorics.Collections;
using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class DynamicBuilding
{
	private readonly string _name;
	private readonly BuildingSection[] _bottoms;
	private readonly BuildingSection[] _mids;
	private readonly BuildingSection[] _tops;
	private readonly JigsawTileType _jigsawJigsawTileType;

	private const int MaxVariationsPerHeight = 100;
	private static readonly Random Rng = new();

	private DynamicBuilding(string name, JigsawTileType jigsawTileType, BuildingSection[] bottoms, BuildingSection[] mids, BuildingSection[] tops)
	{
		_name = name;
		_bottoms = bottoms;
		_mids = mids;
		_tops = tops;
		_jigsawJigsawTileType = jigsawTileType;
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

	public IEnumerable<BuildingInfo> ConstructDynamicBuilding(int minHeight, int maxHeight)
	{
		for (var height = minHeight; height <= maxHeight; height++)
		{
			foreach (var building in ConstructDynamicBuilding(height))
			{
				yield return building;
			}
		}
	}

	private IEnumerable<BuildingInfo> ConstructDynamicBuilding(int height)
	{
		var sets = GetBuildingSets(height);

		var counter = 0;
		foreach (var set in sets)
		{
			counter++;
			var building = set.Aggregate(AddNbtToTop);

			building.RotateBuildingJigsaws(true);
			building.UpdateJigsaws();

			var fileName = $"{_name}-h{height}-{counter}";

			if (_jigsawJigsawTileType == JigsawTileType.BuildingLong)
			{
				var extensionName = $"{fileName}-extension";

				(building, var extension) = building.SplitLong(extensionName);

				extension.FillEmptySpace();
				extension.DebugPrint();
				extension.SaveNbt(extensionName);

				yield return new BuildingInfo
				{
					Name = extensionName,
					Source = _name,
					Height = height,
					JigsawTileType = JigsawTileType.BuildingLongExtension
				};
			}

			building.FillEmptySpace();
			building.DebugPrint();
			building.SaveNbt(fileName);

			yield return new BuildingInfo
			{
				Name = fileName,
				Source = _name,
				Height = height,
				JigsawTileType = _jigsawJigsawTileType
			};
		}
	}
	
	private IEnumerable<List<BuildingSection>> GetBuildingSets(int height)
	{
		var bottomAndTops = GetBottomAndTops().ToArray();
		var midHeight = height - 2;

		var midVariations = new Variations<BuildingSection>(_mids, midHeight + 1, GenerateOption.WithRepetition)
			.Select(bs => bs.ToArray())
			.ToArray();

		var allSets = new (BuildingSection bottom, BuildingSection top, BuildingSection[] mid)[bottomAndTops.Length * midVariations.Length];
		var index = 0;
		foreach (var (bottom, top) in bottomAndTops)
		{
			foreach (var mid in midVariations)
			{
				allSets[index++] = (bottom, top, mid);
			}
		}

		FisherYatesShuffle(allSets);
		var count = Math.Min(MaxVariationsPerHeight, allSets.Length);

		for (var i = 0; i < count; i++)
		{
			var (bottom, top, mid) = allSets[i];
			var sections = new List<BuildingSection> { bottom.Clone() };
			sections.AddRange(mid.Select(m => m.Clone()));
			sections.Add(top.Clone());
			yield return sections;
		}
	}

	private IEnumerable<(BuildingSection bottom, BuildingSection top)> GetBottomAndTops()
	{
		return
			from bottom in _bottoms
			from top in _tops
			select (bottom, top);
	}

	private static void FisherYatesShuffle<T>(T[] array)
	{
		for (var i = array.Length - 1; i > 0; i--)
		{
			var j = Rng.Next(i + 1);
			(array[i], array[j]) = (array[j], array[i]);
		}
	}
	
	private BuildingSection AddNbtToTop(BuildingSection section1, BuildingSection section2)
	{
		var (startX, startY, startZ) = section1.RootTag.GetNbtDimensions();
		var (newX, newY, newZ) = section2.RootTag.GetNbtDimensions();

		if (startX != newX || startZ != newZ)
		{
			Console.Error.WriteLine("Sizes do not match up adding NbtToTop");
			return section1;
		}
		
		section1.RootTag.SetNbtDimensions(startX, startY + newY, startZ);

		if (section1.RootTag.Get<NbtList>("palette") is not { } section1Pallete)
		{
			Console.Error.WriteLine("Section1 does not have a palette");
			return section1;
		}
		
		if (section2.RootTag.Get<NbtList>("palette")?.Clone() is not NbtList section2Pallete)
		{
			Console.Error.WriteLine("Section2 does not have a palette");
			return section1;
		}

		var startPaletteCountCount = section1Pallete.Count;
		
		section1Pallete.AddRange(section2Pallete);
		
		if (section1.RootTag.Get<NbtList>("blocks") is not { } section1Blocks)
		{
			Console.Error.WriteLine("Section1 does not have blocks");
			return section1;
		}
		
		if (section2.RootTag.Get<NbtList>("blocks")?.Clone() is not NbtList section2Blocks)
		{
			Console.Error.WriteLine("Section2 does not have blocks");
			return section1;
		}

		foreach (var section2Block in section2Blocks)
		{
			if (section2Block is not NbtCompound block)
			{
				Console.Error.WriteLine("Section2 block is not a compound");
				continue;
			}
			
			if (block.Get<NbtInt>("state") is not { } state)
			{
				Console.Error.WriteLine("Section2 block does not have a state");
				continue;
			}

			state.Value = state.IntValue + startPaletteCountCount;

			var (x, y, z) = block.GetNbtPosition();
			block.SetNbtPosition(x, y + startY, z);
		}

		section1Blocks.AddRange(section2Blocks);
		
		return section1;
	}
}
