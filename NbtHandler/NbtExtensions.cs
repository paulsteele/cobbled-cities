using fNbt;

namespace Minecraft.City.Datapack.Generator.NbtHandler;

public static class NbtExtensions
{
	private static (int x, int y, int z) GetXyzFromCompound(NbtCompound compound, string fieldName)
	{
		if (!compound.Contains(fieldName))
		{
			return default;
		}

		var sizeNbt = compound.Get<NbtList>(fieldName);

		if (sizeNbt is not { Count: 3 })
		{
			return default;
		}

		var x = sizeNbt[0].IntValue;
		var y = sizeNbt[1].IntValue;
		var z = sizeNbt[2].IntValue;

		return (x, y, z);
	}
	
	public static (int x, int y, int z) GetNbtDimensions(this NbtCompound compound)
	{
		return GetXyzFromCompound(compound, "size");
	}
	
	public static (int x, int y, int z) GetNbtPosition(this NbtCompound compound)
	{
		return GetXyzFromCompound(compound, "pos");
	}

	public static bool IsJigsaw(this NbtCompound compound)
	{
		var nbt = compound.Get<NbtCompound>("nbt");

		if (nbt == null)
		{
			return false;
		}
		
		var tags = new[]
		{
			"joint",
			"name",
			"pool",
			"final_state",
			"id",
			"target"
		};

		return tags.All(s => nbt.Contains(s));
	}

	private static NbtTag GetRoot(this NbtTag tag)
	{
		while (true)
		{
			if (tag.Parent == null)
			{
				return tag;
			}

			tag = tag.Parent;
		}
	}

	private static int GetState(this NbtCompound compound)
	{
		var ret = compound.Get<NbtInt>("state");

		return ret?.Value ?? int.MinValue;
	}

	private static NbtList GetPalette(this NbtCompound compound)
	{
		return compound.Get<NbtList>("palette") ?? new NbtList();
	}

	public static string GetPaletteTag(this NbtCompound block, string tag)
	{
		if (block.GetRoot() is not NbtCompound root)
		{
			return string.Empty;
		}

		var state = block.GetState();
		var palette = root.GetPalette();

		if (palette.Count < state)
		{
			return string.Empty;
		}

		if (palette[state] is not NbtCompound paletteCompound)
		{
			return string.Empty;
		}

		return paletteCompound.Get<NbtCompound>("Properties")?.Get<NbtString>(tag)?.Value ?? string.Empty;

	}
}