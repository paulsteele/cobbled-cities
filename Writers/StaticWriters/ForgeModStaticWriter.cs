namespace Minecraft.City.Datapack.Generator.Writers.StaticWriters;

public class ForgeModStaticWriter : StaticWriter
{
	public override string Path => "META-INF";
	public override string FileName => "neoforge.mods.toml";

	protected override string Contents => """
modLoader="lowcodefml"
loaderVersion="[1,)"
license="MIT"

[[mods]]
modId="poke_cities"
version="1.0"
displayName="Poke Cities"
description='''
Spawn Poke Cities Around Minecraft!
'''
""";
}