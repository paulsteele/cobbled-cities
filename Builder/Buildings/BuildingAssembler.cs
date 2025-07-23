using Combinatorics.Collections;
using fNbt;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;
using Minecraft.City.Datapack.Generator.Writers;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingAssembler(JsonWriter writer) : IAssembler
{
	private const string DynamicInputPath = "../../../nbts/buildings/dynamic";
	private const string DynamicBuildingBottomName = "bottom";
	private const string DynamicBuildingMidName = "mid";
	private const string DynamicBuildingTopName = "top";
	private const string OutputPath = "output/data/poke-cities/structure/buildings";

	public void Assemble()
	{
		var dynamicBuildings = new DirectoryInfo(DynamicInputPath).GetDirectories()
			.Select(DynamicBuilding.GetDynamicBuilding)
			.OfType<DynamicBuilding>()
			.ToArray();

		foreach (var dynamicBuilding in dynamicBuildings)
		{
			const string outputPath = $"output/data/poke-cities/structure/buildings";
			if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}

			dynamicBuilding.Bottom.UpdateJigsaws();
			dynamicBuilding.Bottom.FillEmptySpace();
			dynamicBuilding.Bottom.SaveNbt(dynamicBuilding.Name);
		}
		
		var templatePool = CreateTemplatePool("buildings", dynamicBuildings);
		
		writer.Serialize(templatePool);
	}
	
	private TemplatePool CreateTemplatePool(string fileName, DynamicBuilding[] dynamicBuildings)
	{
		return new TemplatePool(
			"data/poke-cities/worldgen/template_pool",
			$"{fileName}",
			dynamicBuildings.Select(b => 
				new TemplatePoolElementWeight($"poke-cities:buildings/{b.Name}", 1)
			).ToArray()
		);
	}
	public IEnumerable<IEnumerable<string>> AssembleBuildings(int height)
	{
		var parts = new DirectoryInfo("../../../nbts/parts");
		var buildings = parts.GetDirectories();

		return buildings.Select(d => AssembleBuilding(d, height)).Where(s => s.Any()).ToArray();
	}

	private IEnumerable<string> AssembleBuilding(DirectoryInfo directory, int height)
	{
		var files = directory.GetFiles();

		var bases = files.Where(f => f.Name.Contains("base")).Select(f => new NbtFile(f.FullName)).ToArray();
		var mids = files.Where(f => f.Name.Contains("mid")).Select(f => new NbtFile(f.FullName)).ToArray();

		if (!bases.Any())
		{
			Console.WriteLine($"{directory.FullName} does not contain any files with 'base' in the name");
			return Array.Empty<string>();
		}
		
		if (!mids.Any())
		{
			Console.WriteLine($"{directory.FullName} does not contain any files with 'mid' in the name");
			return Array.Empty<string>();
		}

		var allVariations = new List<Variations<NbtFile>>();

		for (var maxHeight = 0; maxHeight < height; maxHeight++)
		{
			allVariations.Add(new Variations<NbtFile>(mids, maxHeight + 1, GenerateOption.WithRepetition));
		}

		var midOptions = allVariations.SelectMany(v => v).ToArray();


		var resultNbtFiles = bases.Aggregate(
			new List<string>(), 
			(current, baseNbt) => current.Concat(
				midOptions.Select(l => ConsolidateNbts(baseNbt, l))
			).ToList()
		);

		return resultNbtFiles;
	}

	private string ConsolidateNbts(NbtFile baseNbt, IEnumerable<NbtFile> additions)
	{
		var additionsArray = additions as NbtFile[] ?? additions.ToArray();
		
		var newNbt = new NbtFile(new NbtCompound(baseNbt.RootTag));
		
		var baseName = new FileInfo(baseNbt.FileName).Name.Replace(".nbt", "");
		var additionName = string.Join('_', additionsArray.Select(f => new FileInfo(f.FileName).Name.Replace(".nbt", "")));

		var location = $"{baseName}_{additionName}";
		var path = $"{OutputPath}/{location}.nbt";
		
		additionsArray.Aggregate(newNbt, AddNbtToTop).SaveToFile(path, NbtCompression.GZip);

		return location;
	}

	private NbtFile AddNbtToTop(NbtFile start, NbtFile newNbt)
	{
		var (startX, startY, startZ) = start.RootTag.GetNbtDimensions();
		var (newX, newY, newZ) = start.RootTag.GetNbtDimensions();

		if (startX != newX || startZ != newZ)
		{
			Console.Error.WriteLine("Sizes do not match up adding NbtToTop");
			return start;
		}

		start.RootTag.Get<NbtList>("size")[1] = new NbtInt(startY + newY);

		var startPalette = start.RootTag.Get<NbtList>("palette");
		var newPalette = newNbt.RootTag.Get<NbtList>("palette").Clone() as NbtList;

		var startPaletteCountCount = startPalette.Count;
		
		startPalette.AddRange(newPalette);
		
		var startBlocks = start.RootTag.Get<NbtList>("blocks");
		var newBlocks = newNbt.RootTag.Get<NbtList>("blocks").Clone() as NbtList;

		foreach (var newBlock in newBlocks)
		{
			var block = newBlock as NbtCompound;

			var state = block.Get<NbtInt>("state");
			state.Value = state.IntValue + startPaletteCountCount;

			var pos = block.Get<NbtList>("pos")[1] = new NbtInt(block.Get<NbtList>("pos")[1].IntValue + startY);
		}

		startBlocks.AddRange(newBlocks);
		
		return start;
	}
	
	private class DynamicBuilding(string name, BuildingSection bottom, BuildingSection mid, BuildingSection top)
	{
		public string Name { get; } = name;
		public BuildingSection Bottom { get; } = bottom;
		public BuildingSection Mid { get; } = mid;
		public BuildingSection Top { get; } = top;

		public static DynamicBuilding? GetDynamicBuilding(DirectoryInfo baseDirectory)
		{
			var bottomDirectory = new DirectoryInfo(Path.Combine(baseDirectory.FullName, DynamicBuildingBottomName));
			var midDirectory = new DirectoryInfo(Path.Combine(baseDirectory.FullName, DynamicBuildingMidName));
			var topDirectory = new DirectoryInfo(Path.Combine(baseDirectory.FullName, DynamicBuildingTopName));
			
			if (!bottomDirectory.Exists || !midDirectory.Exists || !topDirectory.Exists)
			{
				return null;
			}

			const string searchPattern = "*.nbt";

			var bottomFile = bottomDirectory.GetFiles(searchPattern);
			var midFile = midDirectory.GetFiles(searchPattern);
			var topFile = topDirectory.GetFiles(searchPattern);
			
			if (bottomFile.Length != 1 || midFile.Length != 1 || topFile.Length != 1)
			{
				return null;
			}
			
			return new DynamicBuilding(
				baseDirectory.Name,
				new BuildingSection(new NbtFile(bottomFile[0].FullName).RootTag),
				new BuildingSection(new NbtFile(midFile[0].FullName).RootTag),
				new BuildingSection(new NbtFile(topFile[0].FullName).RootTag)
			);
		}
	}
}