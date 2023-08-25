namespace schematic_to_lost_cities.LostCities;

public class StaticWriter
{
	private string tomlContents = """
modLoader="lowcodefml"
loaderVersion="[40,)"
license="MIT"

[[mods]]
modId="schematic_lostcities"
version="1.0"
displayName="Schematic Lost Cities"
description='''
Schematic Assets for Lost Cities
'''
[[dependencies.testassets]]
    modId="lostcities"
    mandatory=true
    versionRange="[1.18-5.3.6,)"
    ordering="AFTER"
    side="BOTH"
""";

	private string packMcMetaContents = """
{
    "pack": {
        "description": "Lost Cities Schematic",
        "pack_format": 9
    }
}
""";

	private string _emptyFront = """
{
  "xsize": 2,
  "zsize": 16,
  "slices": [
    [
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  "
    ],
    [
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  "
    ],
    [
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  "
    ],
    [
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  ",
      "  "
    ]
  ]
}

""";
	
	public void Serialize()
	{
		var tomlDir = "output/META-INF";
		if (!Directory.Exists(tomlDir))
		{
			Directory.CreateDirectory(tomlDir);
		}
		var packDir = "output";
		if (!Directory.Exists(packDir))
		{
			Directory.CreateDirectory(packDir);
		}
		var partsDir = "output/data/lostcities/lostcities/parts";
		if (!Directory.Exists(packDir))
		{
			Directory.CreateDirectory(packDir);
		}

		File.WriteAllText($"{tomlDir}/mods.toml", tomlContents);
		File.WriteAllText($"{packDir}/pack.mcmeta", packMcMetaContents);
		File.WriteAllText($"{partsDir}/emptyfront.json", _emptyFront);
	}
}