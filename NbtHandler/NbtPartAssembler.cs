using Combinatorics.Collections;
using fNbt;

namespace Minecraft.City.Datapack.Generator.NbtHandler;

public class NbtPartAssembler
{
	private readonly NbtFileFixer _fileFixer;
	private const string OutputPath = "output/data/poke-cities/structures/buildings";

	public NbtPartAssembler(NbtFileFixer fileFixer)
	{
		_fileFixer = fileFixer;
	}
	
	public IEnumerable<string> AssembleBuildings(int height)
	{
		var parts = new DirectoryInfo("../../../nbts/parts");
		var buildings = parts.GetDirectories();

		return buildings.SelectMany(d => AssembleBuilding(d, height)).Where(s => s.Any()).ToArray();
	}

	private IEnumerable<string> AssembleBuilding(DirectoryInfo directory, int height)
	{
		var files = directory.GetFiles();

		var bases = files.Where(f => f.Name.Contains("base")).Select(f => new NbtFile(f.FullName)).Select(_fileFixer.FixFile).ToArray();
		var mids = files.Where(f => f.Name.Contains("mid")).Select(f => new NbtFile(f.FullName)).Select(_fileFixer.FixFile).ToArray();

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
		var startX = start.RootTag.Get<NbtList>("size")?[0].IntValue;
		var startY = start.RootTag.Get<NbtList>("size")?[1].IntValue;
		var startZ = start.RootTag.Get<NbtList>("size")?[2].IntValue;
		var newX = newNbt.RootTag.Get<NbtList>("size")?[0].IntValue;
		var newY = newNbt.RootTag.Get<NbtList>("size")?[1].IntValue;
		var newZ = newNbt.RootTag.Get<NbtList>("size")?[2].IntValue;

		if (startX != newX || startZ != newZ)
		{
			Console.Error.WriteLine("Sizes do not match up adding NbtToTop");
			return start;
		}

		start.RootTag.Get<NbtList>("size")[1] = new NbtInt(startY.Value + newY.Value);

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

			var pos = block.Get<NbtList>("pos")[1] = new NbtInt(block.Get<NbtList>("pos")[1].IntValue + startY.Value);
		}

		startBlocks.AddRange(newBlocks);
		
		return start;
	}
}