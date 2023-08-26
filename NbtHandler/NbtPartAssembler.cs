using Combinatorics.Collections;
using fNbt;

namespace Minecraft.City.Datapack.Generator.NbtHandler;

public class NbtPartAssembler
{
	private readonly NbtFileFixer _fileFixer;

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
			new List<NbtFile>(), 
			(current, baseNbt) => current.Concat(
				midOptions.Select(l => ConsolidateNbts(baseNbt, l))
			).ToList()
		);

		return resultNbtFiles.Select(n => n.FileName);
	}

	private NbtFile ConsolidateNbts(NbtFile baseNbt, IEnumerable<NbtFile> additions)
	{
		return additions.Aggregate(baseNbt, AddNbtToTop);
	}

	private NbtFile AddNbtToTop(NbtFile start, NbtFile newNbt)
	{

		return start;
	}
}