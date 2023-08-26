using fNbt;

namespace Minecraft.City.Datapack.Generator.NbtHandler;

public class NbtFileFixer
{
	private readonly (string fromPool, string toPool)[] _replacements = 
	{
		("minecraft:r", "poke-cities:roads"),
		("minecraft:b", "poke-cities:buildings"),
	};
	
	public NbtFileFixer() { }

	public void CopyAndFixFiles()
	{
		var directory = new DirectoryInfo("../../../nbts");

		var files = directory.GetFiles("*.*", SearchOption.AllDirectories);

		foreach (var file in files)
		{
			var directoryName = file.Directory.Name;
			var fileName = file.Name;

			var destinationDirectory = $"output/data/poke-cities/structures/{directoryName}";

			if (!Directory.Exists(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}

			CopyAndFixFile(file.ToString(), $"{destinationDirectory}/{fileName}");
		}
	}
	private void CopyAndFixFile(string fromPath, string toPath)
	{
		var nbt = new NbtFile(fromPath);

		var jigsawBlocks = nbt.RootTag
			.Get<NbtList>("blocks")
			.Cast<NbtCompound>()
			.Where(b => "minecraft:jigsaw".Equals(b.Get<NbtCompound>("nbt")?.Get<NbtString>("id")?.Value));

		foreach (var block in jigsawBlocks)
		{
			var blockNbt = block.Get<NbtCompound>("nbt");
			if (blockNbt == null)
			{
				Console.WriteLine($"Jigsaw block in {fromPath} doesn't have nbt tag");
				continue;
			}
			
			var joint = blockNbt.Get<NbtString>("joint");
			var name = blockNbt.Get<NbtString>("name");
			var pool = blockNbt.Get<NbtString>("pool");

			if (joint == null)
			{
				Console.WriteLine($"Jigsaw block in {fromPath} doesn't have nbt.joint tag");
				continue;
			}
			
			if (name == null)
			{
				Console.WriteLine($"Jigsaw block in {fromPath} doesn't have nbt.name tag");
				continue;
			}
			
			if (pool == null)
			{
				Console.WriteLine($"Jigsaw block in {fromPath} doesn't have nbt.pool tag");
				continue;
			}

			var previousJoint = joint.Value;
			joint.Value = "Aligned";
			Console.WriteLine($"Updated Jigsaw joint in {fromPath} from {previousJoint} to {joint.Value}");

			name.Value = GetJigsawReplacement(name.Value, "name", fromPath);
			pool.Value = GetJigsawReplacement(pool.Value, "pool", fromPath);
		}

		nbt.SaveToFile(toPath, NbtCompression.GZip);
		Console.WriteLine($"Saved {toPath}");
	}

	private string GetJigsawReplacement(string value, string name, string path)
	{
		foreach (var replacement in _replacements)
		{
			if (!value.Equals(replacement.fromPool))
			{
				continue;
			}
			Console.WriteLine($"Updated Jigsaw {name} in {path} from {value} to {replacement.toPool}");
			return replacement.toPool;
		}

		return value;
	}
}