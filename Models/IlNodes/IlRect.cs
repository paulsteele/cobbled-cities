using Minecraft.City.Datapack.Generator.Builder.Jigsaw;

namespace Minecraft.City.Datapack.Generator.Models.IlNodes;

public class IlRect
{
	public IlPoint MinPoint { get; set; }
	public IlPoint MaxPoint { get; set; }

	private IlRect(IlPoint min, IlPoint max)
	{
		MinPoint = min;
		MaxPoint = max;
	}

	public IlRect(int minX, int minZ, int maxX, int maxZ) : this(new IlPoint(minX, minZ), new IlPoint(maxX, maxZ)){}

	public bool PointInside(IlPoint point)
	{
		return PointInside(point.X, point.Z);
	}
	
	public bool PointInside(int x, int z)
	{
		return x >= MinPoint.X && x <= MaxPoint.X && z >= MinPoint.Z && z <= MaxPoint.Z;
	}

	public void ForEach(Action<int, int> action)
	{
		for (var x = MinPoint.X; x <= MaxPoint.X; x++)
		{
			for (var z = MinPoint.Z; z <= MaxPoint.Z; z++)
			{
				action(x, z);
			}
		}
	}

	public int Height => MaxPoint.Z - MinPoint.Z;
	public int Width => MaxPoint.X - MinPoint.X;

	public override string ToString() => $"[{MinPoint}, {MaxPoint}]";

	public (IlRect one, IlRect two) Split(Jigsaw divider)
	{
		IlRect rectOne;
		IlRect rectTwo;
		switch (divider.TileType)
		{
			case JigsawTileType.West:
				rectOne = new IlRect(divider.Location.X, MinPoint.Z, MaxPoint.X, MaxPoint.Z);
				rectTwo = new IlRect(MinPoint.X, MinPoint.Z, divider.Location.X - 1, MaxPoint.Z);
				break;
			// case JigsawTileType.North:
			// 	break;
			// case JigsawTileType.East:
			// 	break;
			// case JigsawTileType.South:
			// 	break;
			default:
				throw new ArgumentOutOfRangeException($"{divider.TileType} not supported in {nameof(IlRect)}.{nameof(Split)}");
		}

		return (rectOne, rectTwo);
	}
}
