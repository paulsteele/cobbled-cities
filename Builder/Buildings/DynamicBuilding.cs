using Combinatorics.Collections;
using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class DynamicBuilding(string name, BuildingSection[] bottoms, BuildingSection[] mids, BuildingSection[] tops)
{
	private const string DynamicInputPath = "../../../nbts/buildings/dynamic";
	private const string DynamicBuildingBottomName = "bottom";
	private const string DynamicBuildingMidName = "mid";
	private const string DynamicBuildingTopName = "top";

	public static IEnumerable<DynamicBuilding> GetAllDynamicBuildings()
	{
		var baseDirectory = new DirectoryInfo(DynamicInputPath);
		if (!baseDirectory.Exists)
		{
			return [];
		}

		return baseDirectory.GetDirectories()
			.Select(GetDynamicBuilding)
			.OfType<DynamicBuilding>()
			.ToArray();
	}
	
	private static DynamicBuilding? GetDynamicBuilding(DirectoryInfo baseDirectory)
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
			GetBuildingSections(bottomFiles),
			GetBuildingSections(midFiles),
			GetBuildingSections(topFiles)
		);
	}

	private static BuildingSection[] GetBuildingSections(FileInfo[] files) =>
		files.Select(f => new BuildingSection(new NbtFile(f.FullName).RootTag)).ToArray();

	public IEnumerable<BuildingInfo> ConstructDynamicBuilding(int minHeight, int maxHeight)
	{
		List<BuildingInfo> buildings = [];
		for(var height = minHeight; height <= maxHeight; height++)
		{
			buildings.AddRange(ConstructDynamicBuilding(height));
		}

		return buildings;
	}

	private IEnumerable<BuildingInfo> ConstructDynamicBuilding(int height)
	{
		var sets = GetBuildingSets(height);

		var counter = 0;
		foreach (var set in sets)
		{
			counter++;
			var building = set.Aggregate(AddNbtToTop);
			
			building.UpdateJigsaws();
			building.FillEmptySpace();
			
			var fileName = $"{name}-h{height}-{counter}";
			building.SaveNbt(fileName);
			
			yield return new BuildingInfo
			{
				Name = fileName,
				Height = height
			};
		}
	}
	
	private IEnumerable<List<BuildingSection>> GetBuildingSets(int height)
	{
		var bottomAndTops = GetBottomAndTops();
		var midHeight = height - 2;

		var midVariations = new Variations<BuildingSection>(mids, midHeight + 1, GenerateOption.WithRepetition)
			.Select(bs => bs)
			.ToArray();
		
		foreach (var (bottom, top) in bottomAndTops)
		{
			var sections = new List<BuildingSection>();
			foreach (var mid in midVariations)
			{
				sections.Add(bottom.Clone());
				sections.AddRange(mid.Select(m => m.Clone()));
				sections.Add(top.Clone());
				yield return sections;
			}
		}
	}

	private IEnumerable<(BuildingSection bottom, BuildingSection top)> GetBottomAndTops()
	{
		return 
			from bottom in bottoms 
			from top in tops 
			select (bottom, top);
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