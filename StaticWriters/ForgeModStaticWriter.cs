namespace Minecraft.City.Datapack.Generator.StaticWriters;

public class ForgeModStaticWriter : StaticWriter
{
	protected override string Path => "META-INF";
	protected override string FileName => "mods.toml";

	protected override string Contents => """
modLoader="lowcodefml"
loaderVersion="[40,)"
license="MIT"

[[mods]]
modId="cityDatapackGeneratorPokeCities"
version="1.0"
displayName="Poke Cities"
description='''
Spawn Poke Cities Around Minecraft!
'''
""";
}