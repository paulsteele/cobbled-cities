namespace Minecraft.City.Datapack.Generator.Tiling.Roads;

public class IlPoint
{
	public int X { get; set; }
	public int Z { get; set; }

	public IlPoint(int x, int z)
	{
		X = x;
		Z = z;
	}

	public static IlPoint operator -(IlPoint a, IlPoint b)
	{
		return new IlPoint(a.X - b.X, a.Z - b.Z);
	}

	public override string ToString() => $"({X}, {Z})";
}