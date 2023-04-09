using fNbt;

namespace schematic_to_lost_cities.Schematic;

public class PaletteEquality : IEqualityComparer<NbtCompound>
{
	public bool Equals(NbtCompound x, NbtCompound y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (ReferenceEquals(x, null)) return false;
		if (ReferenceEquals(y, null)) return false;
		if (x.GetType() != y.GetType()) return false;
		if (x.Count != y.Count) return false;
			
		var nameMatches = x.Get<NbtString>("Name").StringValue.Equals(y.Get<NbtString>("Name").StringValue);

		if (x.Count == 1 || !nameMatches)
		{
			return nameMatches;
		}

		var xProp = x.Get<NbtCompound>("Properties").ToArray();
		var yProp = y.Get<NbtCompound>("Properties").ToArray();

		if (xProp.Length != yProp.Length)
		{
			return false;
		}

		var matches = true;

		for (var i = 0; i < xProp.Length; i++)
		{
			if (xProp[i].TagType != yProp[i].TagType)
			{
				matches = false;
				break;
			}

			if (!xProp[i].StringValue.Equals(yProp[i].StringValue))
			{
				matches = false;
				break;
			}
		}


		return matches;
	}

	public int GetHashCode(NbtCompound obj)
	{
		return HashCode.Combine((int)obj.TagType, obj.Names, obj.Tags, obj.Count);
	}
}