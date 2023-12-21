namespace Minecraft.City.Datapack.Generator.Models.IlNodes;

public class IlRect
{
	public IlPoint MinPoint { get; set; }
	public IlPoint MaxPoint { get; set; }

	public IlRect(IlPoint min, IlPoint max)
	{
		MinPoint = min;
		MaxPoint = max;
	}

	public IlRect(int minX, int minZ, int maxX, int maxZ)
	{
		MinPoint = new IlPoint(minX, minZ);
		MaxPoint = new IlPoint(maxX, maxZ);
	}

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
}