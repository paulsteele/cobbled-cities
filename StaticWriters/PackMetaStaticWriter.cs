namespace Minecraft.City.Datapack.Generator.StaticWriters;

public class PackMetaStaticWriter : StaticWriter
{
	protected override string Path => "";
	protected override string FileName => "pack.mcmeta";

	protected override string Contents => """
{
	"pack": {
		"description": "Poke Cities",
		"pack_format": 10
	}
}
""";
}