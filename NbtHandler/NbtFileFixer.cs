using fNbt;

namespace Minecraft.City.Datapack.Generator.NbtHandler;

public class NbtFileFixer
{
	private readonly (string fromPool, string toPool)[] _replacements = 
	{
		("minecraft:r", "poke-cities:roads"),
		("minecraft:2r", "poke-cities:tworoad"),
		("minecraft:2i", "poke-cities:twointer"),
		("minecraft:5r", "poke-cities:fiveroad"),
		("minecraft:5i", "poke-cities:fiveinter"),
		("minecraft:p", "poke-cities:park"),
		("minecraft:b", "poke-cities:buildings"),
	};

	public NbtFile FixFile(NbtFile nbt)
	{
		var jigsawBlocks = nbt.RootTag
			.Get<NbtList>("blocks")
			.Cast<NbtCompound>()
			.Where(b => "minecraft:jigsaw".Equals(b.Get<NbtCompound>("nbt")?.Get<NbtString>("id")?.Value));

		Console.WriteLine($"Updating Jigsaw {nbt.FileName}");
		
		foreach (var block in jigsawBlocks)
		{
			var blockNbt = block.Get<NbtCompound>("nbt");
			if (blockNbt == null)
			{
				Console.WriteLine($"Jigsaw block in {nbt.FileName} doesn't have nbt tag");
				continue;
			}
			
			var joint = blockNbt.Get<NbtString>("joint");
			var name = blockNbt.Get<NbtString>("name");
			var pool = blockNbt.Get<NbtString>("pool");
			var target = blockNbt.Get<NbtString>("target");

			if (joint == null)
			{
				Console.WriteLine($"Jigsaw block in {nbt.FileName} doesn't have nbt.joint tag");
				continue;
			}
			
			if (name == null)
			{
				Console.WriteLine($"Jigsaw block in {nbt.FileName} doesn't have nbt.name tag");
				continue;
			}
			
			if (pool == null)
			{
				Console.WriteLine($"Jigsaw block in {nbt.FileName} doesn't have nbt.pool tag");
				continue;
			}
			
			if (target == null)
			{
				Console.WriteLine($"Jigsaw block in {nbt.FileName} doesn't have nbt.target tag");
				continue;
			}

			var previousJoint = joint.Value;
			joint.Value = "Aligned";
			Console.WriteLine($"Updated Jigsaw joint from {previousJoint} to {joint.Value}");

			name.Value = GetJigsawReplacement(name.Value, "name", nbt.FileName);
			pool.Value = GetJigsawReplacement(pool.Value, "pool", nbt.FileName);
			target.Value = GetJigsawReplacement(target.Value, "target", nbt.FileName);
		}

		return nbt;
	}

	private string GetJigsawReplacement(string value, string name, string path)
	{
		foreach (var replacement in _replacements)
		{
			if (!value.Equals(replacement.fromPool))
			{
				continue;
			}
			Console.WriteLine($"Updated Jigsaw {name} from {value} to {replacement.toPool}");
			return replacement.toPool;
		}

		return value;
	}
}