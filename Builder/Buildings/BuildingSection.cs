using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.IlNodes;

namespace Minecraft.City.Datapack.Generator.Builder.Buildings;

public class BuildingSection(NbtCompound rootTag) : AbstractSection(rootTag)
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

	public BuildingSection SplitLong(string extensionPoolName)
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

		var jigsawPosition = (
			x: boundary.X + oneEighty.GetOffsetForTileType().x + twoSeventy.GetOffsetForTileType().x,
			y: jigsaw.Compound.GetNbtPosition().y - 1,
			z:boundary.Z + oneEighty.GetOffsetForTileType().z + twoSeventy.GetOffsetForTileType().z
		);
		var extensionJigsawPosition = (
			x: boundary.X + oneEighty.GetOffsetForTileType().x,
			y: jigsaw.Compound.GetNbtPosition().y - 1,
			z: boundary.Z + oneEighty.GetOffsetForTileType().z
		);

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

			return
				nbtPosition.x == jigsawPosition.x &&
				nbtPosition.y == jigsawPosition.y &&
				nbtPosition.z == jigsawPosition.z;
		});
		
		var originalExtensionJigsawBlock = blocks.OfType<NbtCompound>().First(b =>
		{
			var nbtPosition = b.GetNbtPosition();

			return
				nbtPosition.x == extensionJigsawPosition.x &&
				nbtPosition.y == extensionJigsawPosition.y &&
				nbtPosition.z == extensionJigsawPosition.z;
		});

		var originalJigsawName =
			((NbtCompound)palette[originalJigsawBlock["state"].IntValue]).Get<NbtString>("Name")?.Value ?? string.Empty;
		var originalExtensionJigsawName = 
			((NbtCompound)palette[originalExtensionJigsawBlock["state"].IntValue]).Get<NbtString>("Name")?.Value ?? string.Empty;

		originalJigsawBlock.MakeJigsaw(finalState: originalJigsawName);
		originalExtensionJigsawBlock.MakeJigsaw(finalState: originalExtensionJigsawName);
		
		originalJigsawBlock.SetState(states[ninety]);
		originalExtensionJigsawBlock.SetState(states[twoSeventy]);

		var newJigsaw = new Jigsaw.Jigsaw(originalJigsawBlock, RootTag, new IlPoint(jigsawPosition.x, jigsawPosition.z));
		var newExtensionJigsaw = new Jigsaw.Jigsaw(originalExtensionJigsawBlock, RootTag, new IlPoint(extensionJigsawPosition.x, extensionJigsawPosition.z));
		
		newJigsaw.SetJigsawPool(extensionPoolName);
		newJigsaw.SetJigsawTarget(extensionPoolName);
		
		newExtensionJigsaw.SetJigsawName(extensionPoolName);

		Jigsaws.Add(newJigsaw.Location, newJigsaw);
		Jigsaws.Add(newExtensionJigsaw.Location, newExtensionJigsaw);
		
		return null;
	}
}
