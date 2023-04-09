using fNbt;

namespace schematic_to_lost_cities;

// ReSharper disable once UnusedType.Global
public static class Program
{
	// ReSharper disable once UnusedMember.Global
	public static void Main()
	{
		var schematic = new Schematic.Schematic(new NbtFile("pokecenter.nbt"));
		Console.WriteLine("test");
	}
}