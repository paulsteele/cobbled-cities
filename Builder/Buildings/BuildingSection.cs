using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public record LongSplitInfo(
	IlRect OriginalRect,
	IlRect ExtensionRect,
	int JigsawX,
	int JigsawZ,
	int ExtensionJigsawX,
	int ExtensionJigsawZ,
	JigsawTileType JigsawDirection,
	JigsawTileType ExtensionJigsawDirection,
	JigsawTileType OneEightyDirection
);

public class BuildingSection(
	NbtCompound rootTag,
	IlRect? boundingBox = null,
	Dictionary<IlPoint, Jigsaw.Jigsaw>? rootJigsaws = null
) : AbstractSection(rootTag, boundingBox, rootJigsaws)
{
	public string SaveNbt(string fileName)
	{
		var outputPath = $"output/data/cobbled-cities/structure/buildings";
		if (!Directory.Exists(outputPath))
		{
			Directory.CreateDirectory(outputPath);
		}

		var path = $"{outputPath}/{fileName}.nbt";

		var nbt = new NbtFile(RootTag);

		nbt.SaveToFile(path, NbtCompression.GZip);

		Console.WriteLine($"Saved {path}");

		return path;
	}

	public void UpdateJigsaws()
	{
		foreach (var jigsaw in Jigsaws.Values)
		{
			jigsaw.SetJigsawName("cobbled-cities:buildings-start");
		}
	}

	public BuildingSection Clone()
	{
		return new BuildingSection(RootTag);
	}

	public void AddVerticalJigsaws(string source, FloorType floorType, int? chainStep)
	{
		var blocks = RootTag.Get<NbtList>("blocks");
		var palette = RootTag.GetPalette();
		var states = RootTag.GetTypeStateIds();

		if (blocks == null)
		{
			throw new InvalidDataException("blocks can't be null!");
		}

		const int jigsawX = 1;
		const int jigsawZ = 1;

		// Vertical jigsaws are NOT added to the Jigsaws dictionary — they don't
		// participate in horizontal tile logic (RotateBuildingJigsaws, SplitLong, etc.)
		// and two jigsaws at the same (X,Z) but different Y would collide in the dict.

		if (floorType is FloorType.Bottom or FloorType.Mid)
		{
			var upY = MaxY - 1;
			var upBlock = blocks.OfType<NbtCompound>().First(b =>
			{
				var pos = b.GetNbtPosition();
				return pos.x == jigsawX && pos.y == upY && pos.z == jigsawZ;
			});

			var upBlockName =
				((NbtCompound)palette[upBlock["state"].IntValue]).Get<NbtString>("Name")?.Value ?? string.Empty;
			upBlock.MakeJigsaw(finalState: upBlockName, joint: "aligned");
			upBlock.SetState(states[JigsawTileType.Up]);

			var upJigsaw = new Jigsaw.Jigsaw(upBlock, RootTag, new IlPoint(jigsawX, jigsawZ));

			var upPool = floorType == FloorType.Bottom
				? $"cobbled-cities:floors-mid-{source}-step-{chainStep}"
				: chainStep > 1
					? $"cobbled-cities:floors-mid-{source}-step-{chainStep - 1}"
					: $"cobbled-cities:floors-top-{source}";

			upJigsaw.SetJigsawPool(upPool);
			upJigsaw.SetJigsawTarget("cobbled-cities:floors-up");
		}

		if (floorType is FloorType.Mid or FloorType.Top)
		{
			const int downY = 0;
			var downBlock = blocks.OfType<NbtCompound>().First(b =>
			{
				var pos = b.GetNbtPosition();
				return pos.x == jigsawX && pos.y == downY && pos.z == jigsawZ;
			});

			var downBlockName =
				((NbtCompound)palette[downBlock["state"].IntValue]).Get<NbtString>("Name")?.Value ?? string.Empty;
			downBlock.MakeJigsaw(finalState: downBlockName, joint: "aligned");
			downBlock.SetState(states[JigsawTileType.Down]);

			var downJigsaw = new Jigsaw.Jigsaw(downBlock, RootTag, new IlPoint(jigsawX, jigsawZ));
			downJigsaw.SetJigsawName("cobbled-cities:floors-up");
		}
	}

	public (BuildingSection original, BuildingSection extension, LongSplitInfo splitInfo) SplitLong(string extensionPoolName)
	{
		if (Jigsaws.Count != 1)
		{
			throw new Exception("More than 1 Jigsaw found in long building");
		}

		var (pos, jigsaw) = Jigsaws.First();
		var ninety = jigsaw.TileType.Rotated90DegreesClockwiseTileType();
		var oneEighty = jigsaw.TileType.Rotated180DegreesTileType();
		var twoSeventy = jigsaw.TileType.Rotated180DegreesTileType().Rotated90DegreesClockwiseTileType();

		var boundary = pos.Clone();
		while (HasTile(boundary.X, boundary.Z))
		{
			boundary.X += ninety.GetOffsetForTileType().x;
			boundary.Z += ninety.GetOffsetForTileType().z;
		}

		var jigsawX = boundary.X + oneEighty.GetOffsetForTileType().x + twoSeventy.GetOffsetForTileType().x;
		var jigsawZ = boundary.Z + oneEighty.GetOffsetForTileType().z + twoSeventy.GetOffsetForTileType().z;
		var jigsawY = jigsaw.Compound.GetNbtPosition().y - 1;

		var extensionJigsawX = boundary.X + oneEighty.GetOffsetForTileType().x;
		var extensionJigsawZ = boundary.Z + oneEighty.GetOffsetForTileType().z;

		var (original, extension) = ApplyLongSplit(
			jigsawX, jigsawY, jigsawZ,
			extensionJigsawX, jigsawY, extensionJigsawZ,
			ninety, twoSeventy, oneEighty,
			extensionPoolName
		);

		var splitInfo = new LongSplitInfo(
			original.GetBoundingRect(),
			extension.GetBoundingRect(),
			jigsawX, jigsawZ,
			extensionJigsawX, extensionJigsawZ,
			ninety, twoSeventy, oneEighty
		);

		return (original, extension, splitInfo);
	}

	public (BuildingSection original, BuildingSection extension) SplitLongFloor(
		LongSplitInfo splitInfo,
		string extensionPoolName)
	{
		const int jigsawY = 0;

		return ApplyLongSplit(
			splitInfo.JigsawX, jigsawY, splitInfo.JigsawZ,
			splitInfo.ExtensionJigsawX, jigsawY, splitInfo.ExtensionJigsawZ,
			splitInfo.JigsawDirection, splitInfo.ExtensionJigsawDirection,
			splitInfo.OneEightyDirection,
			extensionPoolName
		);
	}

	private (BuildingSection original, BuildingSection extension) ApplyLongSplit(
		int jigsawX, int jigsawY, int jigsawZ,
		int extJigsawX, int extJigsawY, int extJigsawZ,
		JigsawTileType jigsawDirection,
		JigsawTileType extJigsawDirection,
		JigsawTileType oneEighty,
		string extensionPoolName)
	{
		var blocks = RootTag.Get<NbtList>("blocks");
		var palette = RootTag.GetPalette();
		var states = RootTag.GetTypeStateIds();

		if (blocks == null)
		{
			throw new InvalidDataException("blocks can't be null!");
		}

		var originalJigsawBlock = blocks.OfType<NbtCompound>().First(b =>
		{
			var nbtPosition = b.GetNbtPosition();
			return nbtPosition.x == jigsawX && nbtPosition.y == jigsawY && nbtPosition.z == jigsawZ;
		});

		var originalExtensionJigsawBlock = blocks.OfType<NbtCompound>().First(b =>
		{
			var nbtPosition = b.GetNbtPosition();
			return nbtPosition.x == extJigsawX && nbtPosition.y == extJigsawY && nbtPosition.z == extJigsawZ;
		});

		var originalJigsawName =
			((NbtCompound)palette[originalJigsawBlock["state"].IntValue]).Get<NbtString>("Name")?.Value ?? string.Empty;
		var originalExtensionJigsawName =
			((NbtCompound)palette[originalExtensionJigsawBlock["state"].IntValue]).Get<NbtString>("Name")?.Value ?? string.Empty;

		originalJigsawBlock.MakeJigsaw(finalState: originalJigsawName);
		originalExtensionJigsawBlock.MakeJigsaw(finalState: originalExtensionJigsawName);

		originalJigsawBlock.SetState(states[jigsawDirection]);
		originalExtensionJigsawBlock.SetState(states[extJigsawDirection]);

		var newJigsaw = new Jigsaw.Jigsaw(originalJigsawBlock, RootTag, new IlPoint(jigsawX, jigsawZ));
		var newExtensionJigsaw = new Jigsaw.Jigsaw(originalExtensionJigsawBlock, RootTag, new IlPoint(extJigsawX, extJigsawZ));

		var poolName = $"cobbled-cities:{extensionPoolName}";

		newJigsaw.SetJigsawPool(poolName);
		newJigsaw.SetJigsawTarget(poolName);

		newExtensionJigsaw.SetJigsawName(poolName);

		Jigsaws.Add(newJigsaw.Location, newJigsaw);
		Jigsaws.Add(newExtensionJigsaw.Location, newExtensionJigsaw);

		var fullRect = new IlRect(0, 0, MaxX - 1, MaxZ - 1);

		var (originalRect, extensionRect) = fullRect.Split(newJigsaw);

		extensionRect.MaxPoint.X += oneEighty.GetOffsetForTileType().x;
		extensionRect.MaxPoint.Z += oneEighty.GetOffsetForTileType().z;

		var original = new BuildingSection(RootTag, originalRect, Jigsaws);
		var extension = new BuildingSection(RootTag, extensionRect, Jigsaws);

		return (original, extension);
	}

	private IlRect GetBoundingRect() => new(0, 0, MaxX - 1, MaxZ - 1);
}
