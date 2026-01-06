using fNbt;
using Minecraft.City.Datapack.Generator.Builder.Buildings;
using Minecraft.City.Datapack.Generator.Builder.Jigsaw;
using Minecraft.City.Datapack.Generator.Models.IlNodes;
using Minecraft.City.Datapack.Generator.Models.TemplatePool;

namespace Minecraft.City.Datapack.Generator.Builder.Roads;

public class RoadSection : AbstractSection
{
	private int _nextSubsectionIndex;

	public RoadSection(NbtCompound rootTag,
		IlRect? boundingBox = null,
		Dictionary<IlPoint, Jigsaw.Jigsaw>? rootJigsaws = null,
		int index = -1
	) : base(BoundCompoundToBoundingBox(rootTag, boundingBox))
	{
		Index = index;

		if (rootJigsaws == null)
		{
			return;
		}
		foreach (var jigsawsValue in Jigsaws.Values)
		{
			if (boundingBox != null)
			{
				jigsawsValue.OriginalLocation = new IlPoint(
					jigsawsValue.Location.X + boundingBox.MinPoint.X,
					jigsawsValue.Location.Z + boundingBox.MinPoint.Z
				);
			}
	
			if (!rootJigsaws.TryGetValue(jigsawsValue.OriginalLocation, out var rootJigsaw))
			{
				continue;
			}
			jigsawsValue.PointingToLocation = rootJigsaw.PointingToLocation;
			jigsawsValue.PointsToOutside = rootJigsaw.PointsToOutside;
			jigsawsValue.PointsFromOutside = rootJigsaw.PointsFromOutside;
		}
	}
	
	private void RotateBuildingJigsaws()
	{
		var states = RootTag.GetTypeStateIds();
		var north = states[JigsawTileType.North];
		var east = states[JigsawTileType.East];
		var south = states[JigsawTileType.South];
		var west = states[JigsawTileType.West];

		foreach (var jigsaw in Jigsaws.Values.Where(j => j.IsBuilding))
		{
			var pos = jigsaw.Location;
			
			var maxX = MaxX - 1;
			var maxZ = MaxZ - 1;
			
			var state = pos switch
			{
				{ X: 0, Z: 0 } => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == 0 && pos.Z == maxZ => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == maxX && pos.Z == 0 => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == maxX && pos.Z == maxZ => throw new ArgumentException($"Building jigsaw in corner {pos} not allowed"),
				_ when pos.X == 0 => west,
				_ when pos.X == maxX => east,
				_ when pos.Z == 0 => north,
				_ when pos.Z == maxZ => south,
				_ => throw new ArgumentException($"Building jigsaw at {pos} not on edge")
			};
			
			jigsaw.Compound.SetState(state);
		}
	}

	private static NbtCompound BoundCompoundToBoundingBox(NbtCompound rootTag, IlRect? boundingBox)
	{
		if (boundingBox == null)
		{
			return rootTag;
		}
		
		var newRootTag = (NbtCompound)rootTag.Clone();
		var (_, maxY, _) = newRootTag.GetNbtDimensions();
		var blocks = newRootTag.Get<NbtList>("blocks");
		
		if (blocks == null)
		{
			throw new ArgumentException($"{nameof(rootTag)} does not have any blocks");
		}
		
		newRootTag.SetNbtDimensions(boundingBox.Width + 1, maxY, boundingBox.Height + 1);

		var tempBlocks = blocks
			.Where(b => b is NbtCompound)
			.Cast<NbtCompound>()
			.Where(b =>
			{
				var pos = b.GetNbtPosition();
				return boundingBox.PointInside(pos.x, pos.z);
			}).ToList();

		foreach (var block in tempBlocks)
		{
			var pos = block.GetNbtPosition();
			block.SetNbtPosition(pos.x - boundingBox.MinPoint.X, pos.y, pos.z - boundingBox.MinPoint.Z);
		}

		var newList = new NbtList("blocks");
		newList.AddRange(tempBlocks);

		newRootTag["blocks"] = newList;
		return newRootTag;
	}
	
	public int Index { get; }

	private bool HasTile(int x, int z)
	{
		if (x < 0 || z < 0)
		{
			return false;
		}

		if (x >= MaxX || z >= MaxZ)
		{
			return false;
		}

		return TileMap[x, z];
	}

	public bool HasSubSections => Jigsaws.Values.Select(j => !j.IsBuilding).Any();

	public RoadSection TakeSubSection()
	{
		var first = Jigsaws.Values.First(j => !j.IsBuilding);

		var coordinates = GetRect(first.Compound);

		var subsection = new RoadSection(RootTag, coordinates, Jigsaws, _nextSubsectionIndex++);

		var toRemove = Jigsaws
			.Where(j => coordinates.PointInside(j.Key))
			.Select(j => j.Key)
			.ToList();

		foreach (var point in toRemove)
		{
			Jigsaws.Remove(point);
		}

		coordinates.ForEach((x, z) => TileMap[x, z] = false);

		return subsection;
	}

	public void UpdateJigsaws
	(
		string baseFileName, 
		Dictionary<IlPoint, int> jigsawPointToIndex, 
		RoadZone roadZone,
		IBuildingZoneService buildingZoneService
	)
	{
		RotateBuildingJigsaws();
		FlipPointedToJigsaws();

		foreach (var jigsaw in Jigsaws.Values)
		{
			jigsaw.SetJigsawName($"cobbled-cities:{baseFileName}-{Index}-{jigsaw.OriginalLocation.SerializedString}");
			if (jigsaw.IsBuilding)
			{
				var distance = Math.Sqrt(
					Math.Pow(jigsaw.OriginalLocation.X - roadZone.Origin.X, 2) + 
					Math.Pow(jigsaw.OriginalLocation.Z - roadZone.Origin.Z, 2)
				);

				var buildingZone = buildingZoneService.GetZoneByDistance(distance);
				jigsaw.SetJigsawPool($"cobbled-cities:{buildingZone.GetNameForType(jigsaw.TileType)}");
				jigsaw.SetJigsawTarget($"cobbled-cities:buildings-start");
				continue;
			}
			if (
				jigsaw.PointingToLocation == null ||
				!jigsawPointToIndex.TryGetValue(jigsaw.PointingToLocation, out var pointIndex)
			)
			{
				if (jigsaw.PointsToOutside)
				{
					jigsaw.SetJigsawPool($"cobbled-cities:{roadZone.NextZone?.Name}");
					jigsaw.SetJigsawTarget($"cobbled-cities:{roadZone.NextZone?.Name}-start");
				}

				if (jigsaw.PointsFromOutside)
				{
					jigsaw.SetJigsawName($"cobbled-cities:{roadZone.Name}-start");
				}
				continue;
			}

			jigsaw.SetJigsawPool($"cobbled-cities:{baseFileName}-{pointIndex}");
			jigsaw.SetJigsawTarget(
				$"cobbled-cities:{baseFileName}-{pointIndex}-{jigsaw.PointingToLocation.SerializedString}");
		}
	}

	private void FlipPointedToJigsaws()
	{
		var states = RootTag.GetTypeStateIds();
		foreach (
			var jigsaw in Jigsaws.Values
				.Where(jigsaw => jigsaw is { IsBuilding: false, PointingToLocation: null, PointsToOutside: false })
			)
		{
			jigsaw.Flip(states);
		}
	}

	private IlRect GetRect(NbtCompound jigsaw)
	{
		// Get pass one delta
		var (xChange, zChange) = jigsaw.GetJigsawTileType(RootTag).GetOffsetForTileType();

		if (xChange == zChange)
		{
			throw new ArgumentException("tile was not a jigsaw");
		}

		var path = new List<IlPoint>();

		var location = jigsaw.GetNbtPosition();

		var oppositeBoundaryCandidate1 = GetBoundaryInDirection(location.x, location.z, xChange, zChange, true, path);
		var oppositeBoundaryCandidate2 = GetBoundaryInDirection(location.x, location.z, -xChange, -zChange, true, path);

		var oppositeBoundary = oppositeBoundaryCandidate1.Equals(new IlPoint(location.x, location.z))
			? oppositeBoundaryCandidate2
			: oppositeBoundaryCandidate1;

		path.Add(oppositeBoundary);

		var crossXChange = Math.Abs(zChange);
		var crossZChange = Math.Abs(xChange);

		var mins = path.Select(i => GetBoundaryInDirection(i.X, i.Z, -crossXChange, -crossZChange, false));
		var maxes = path.Select(i => GetBoundaryInDirection(i.X, i.Z, crossXChange, crossZChange, false));

		var minX = 0;
		var minZ = 0;
		var maxX = 0;
		var maxZ = 0;

		//horizontal
		if (xChange != 0)
		{
			minX = Math.Min(location.x, oppositeBoundary.X);
			maxX = Math.Max(location.x, oppositeBoundary.X);

			minZ = mins.Max(i => i.Z);
			maxZ = maxes.Min(i => i.Z);
		}
		//vertical
		else if (zChange != 0)
		{
			minZ = Math.Min(location.z, oppositeBoundary.Z);
			maxZ = Math.Max(location.z, oppositeBoundary.Z);

			minX = mins.Max(i => i.X);
			maxX = maxes.Min(i => i.X);
		}

		return new IlRect(minX, minZ, maxX, maxZ);
	}

	private IlPoint GetBoundaryInDirection(int startingX, int startingZ, int offsetX, int offsetZ, bool startingOnJigsaw,
		ICollection<IlPoint>? trace = null)
	{
		var allowedToTakeJigsaw = !startingOnJigsaw;
		while (true)
		{
			var newX = startingX + offsetX;
			var newZ = startingZ + offsetZ;

			if (!HasTile(newX, newZ))
			{
				return new IlPoint(startingX, startingZ);
			}

			if (Jigsaws.TryGetValue(new IlPoint(newX, newZ), out var jigsaw))
			{
				switch (jigsaw.TileType)
				{
					case JigsawTileType.North when allowedToTakeJigsaw:
					case JigsawTileType.East when allowedToTakeJigsaw:
					case JigsawTileType.South when allowedToTakeJigsaw:
					case JigsawTileType.West when allowedToTakeJigsaw:
						allowedToTakeJigsaw = false;
						break;
					case JigsawTileType.BuildingNormal:
					case JigsawTileType.BuildingLong:
					case JigsawTileType.BuildingCorner:
						break;
					default:
						return new IlPoint(startingX, startingZ);
				}
			}
			else
			{
				allowedToTakeJigsaw = true;
			}

			trace?.Add(new IlPoint(startingX, startingZ));

			startingX = newX;
			startingZ = newZ;
		}
	}

	public bool IsCenter()
	{
		return Jigsaws.Values.All(j => j.PointingToLocation != null || j.PointsFromOutside || j.IsBuilding);
	}

	public string SaveNbt(string fileName, string typeName)
	{
		var outputPath = $"output/data/cobbled-cities/structure/{typeName}";
		if (!Directory.Exists(outputPath))
		{
			Directory.CreateDirectory(outputPath);
		}

		var path = $"{outputPath}/{fileName}-{Index}.nbt";

		var nbt = new NbtFile(RootTag);

		nbt.SaveToFile(path, NbtCompression.GZip);

		Console.WriteLine($"Saved {path}");

		return path;
	}

	public TemplatePool CreateTemplatePool(string fileName, string typeName)
	{
		return new TemplatePool(
			"data/cobbled-cities/worldgen/template_pool",
			$"{fileName}-{Index}",
			new[]
			{
				new TemplatePoolElementWeight($"cobbled-cities:{typeName}/{fileName}-{Index}", 1)
			}
		);
	}
}
