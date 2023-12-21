namespace Minecraft.City.Datapack.Generator.Builder.Roads;

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

	public int Height => MaxPoint.Z - MinPoint.Z;
	public int Width => MaxPoint.X - MinPoint.X;

	public override string ToString() => $"[{MinPoint}, {MaxPoint}]";
}