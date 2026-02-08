using fNbt;

namespace Minecraft.City.Datapack.Generator.Builder;

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
	
	private static void SetXyzFromCompound(NbtCompound compound, string fieldName, int x, int y, int z)
	{
		if (!compound.Contains(fieldName))
		{
			return;
		}

		var sizeNbt = compound.Get<NbtList>(fieldName);

		if (sizeNbt is not { Count: 3 })
		{
			return;
		}

		sizeNbt[0] = new NbtInt(x);
		sizeNbt[1] = new NbtInt(y);
		sizeNbt[2] = new NbtInt(z);
	}
	public static (int x, int y, int z) GetNbtDimensions(this NbtCompound compound)
	{
		return GetXyzFromCompound(compound, "size");
	}
	
	public static (int x, int y, int z) GetNbtPosition(this NbtCompound compound)
	{
		return GetXyzFromCompound(compound, "pos");
	}
	
	public static void SetNbtDimensions(this NbtCompound compound, int x, int y, int z)
	{
		SetXyzFromCompound(compound, "size", x, y, z);
	}
	
	public static void SetNbtPosition(this NbtCompound compound, int x, int y, int z)
	{
		SetXyzFromCompound(compound, "pos", x, y , z);
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
	
	public static void MakeJigsaw(this NbtCompound compound, string finalState = "minecraft:air")
	{
		var nbt = new NbtCompound("nbt")
		{
			["joint"] = new NbtString("joint", "self"),
			["name"] = new NbtString("name", "minecraft:empty"),
			["pool"] = new NbtString("pool", "minecraft:empty"),
			["final_state"] = new NbtString("final_state", finalState),
			["id"] = new NbtString("id", "minecraft:jigsaw"),
			["target"] = new NbtString("target", "minecraft:empty")
		};

		compound["nbt"] = nbt;
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
	
	public static void SetState(this NbtCompound compound, int value)
	{
		compound["state"] = new NbtInt("state", value);
	}

	public static NbtList GetPalette(this NbtCompound compound)
	{
		return compound.Get<NbtList>("palette") ?? new NbtList();
	}

	public static string GetPalettePropertiesTag(this NbtCompound block, NbtCompound root, string tag)
	{
		var state = block.GetState();
		var palette = root.GetPalette();

		if (palette.Count < state || palette[state] is not NbtCompound paletteCompound)
		{
			return string.Empty;
		}

		return paletteCompound.Get<NbtCompound>("Properties")?.Get<NbtString>(tag)?.Value ?? string.Empty;

	}
	
	public static string GetPaletteTag(this NbtCompound block, NbtCompound root, string tag)
	{
		var state = block.GetState();
		var palette = root.GetPalette();

		if (palette.Count < state || palette[state] is not NbtCompound paletteCompound)
		{
			return string.Empty;
		}

		return paletteCompound.Get<NbtString>(tag)?.Value ?? string.Empty;
	}
}
