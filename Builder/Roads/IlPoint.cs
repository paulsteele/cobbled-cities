namespace Minecraft.City.Datapack.Generator.Builder.Roads;

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

	public IlPoint Clone()
	{
		return new IlPoint(X, Z);
	}

	public override bool Equals(object? obj)
	{
		if (obj is IlPoint other)
		{
			return Equals(other);
		}

		return false;
	}

	private bool Equals(IlPoint other)
	{
		return X == other.X && Z == other.Z;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Z);
	}

	public override string ToString() => $"({X}, {Z})";
}