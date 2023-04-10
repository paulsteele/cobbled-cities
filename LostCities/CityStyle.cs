
using System.Text.Json.Serialization;

namespace schematic_to_lost_cities.LostCities;

public class CityStyle
{
  [JsonPropertyName("inherit")] public string inherit { get; } = "citystyle_config";
  [JsonPropertyName("style")] public string style { get; } = "standard";
  [JsonPropertyName("streetblocks")] public StreetBlocks streetblocks { get; } = new StreetBlocks();
  [JsonPropertyName("parkblocks")] public ParkBlocks parkblocks { get; } = new ParkBlocks();
  [JsonPropertyName("corridorblocks")] public CorridorBlocks corridorblocks { get; } = new CorridorBlocks();
  [JsonPropertyName("railblocks")] public RailBlocks railblocks { get; } = new RailBlocks();
  [JsonPropertyName("sphereblocks")] public SphereBlocks sphereblocks { get; } = new SphereBlocks();
  [JsonPropertyName("selectors")] public Selectors selectors { get; }= new Selectors();

  public void SetBuildings(IEnumerable<Part> parts)
  {
    selectors.buildings = parts.Select(p => new BuildingSelection(1, p.Name)).ToArray();
  }
  
	public void Serialize()
	{
		var paletteDir = "output/data/schematicassets/lostcities/citystyles";
		if (!Directory.Exists(paletteDir))
		{
			Directory.CreateDirectory(paletteDir);
		}
		File.WriteAllText($"{paletteDir}/schematicstyle.json", Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
	}
}

public class StreetBlocks
{
  [JsonPropertyName("border")] public string border { get; } = "y";
  [JsonPropertyName("wall")] public string wall { get; } = "w";
  [JsonPropertyName("street")] public string street { get; } = "S";
  [JsonPropertyName("streetbase")] public string streetbase { get; } = "b";
  [JsonPropertyName("streetvariant")] public string streetvariant { get; } = "B";
}

public class ParkBlocks
{
  [JsonPropertyName("elevation")] public string elevation { get; } = "x";
}

public class CorridorBlocks
{
  [JsonPropertyName("roof")] public string roof { get; }= "x";
  [JsonPropertyName("glass")] public string glass { get; }= "+";
}

public class RailBlocks
{
  [JsonPropertyName("railmain")] public string railmain { get; }= "y";
}

public class SphereBlocks
{
  [JsonPropertyName("glass")] public string glass { get; }= "y";
  [JsonPropertyName("border")] public string border { get; }= "9";
  [JsonPropertyName("inner")] public string inner { get; }= "b";
}

public class Selectors
{
  [JsonPropertyName("buildings")] public BuildingSelection[] buildings { get; set; }
  [JsonPropertyName("multibuildings")] public BuildingSelection[] multibuildings { get; }= Array.Empty<BuildingSelection>();

  [JsonPropertyName("bridges")] public BuildingSelection[] bridges { get; }= {
    new BuildingSelection(1.0m, "bridge_open"),
    new BuildingSelection(1.0m, "bridge_covered"),
  };
  
  [JsonPropertyName("fronts")] public BuildingSelection[] fronts { get; }= {
    new BuildingSelection(1.0m, "bridge_front1"),
    new BuildingSelection(1.0m, "bridge_front2"),
    new BuildingSelection(1.0m, "bridge_front3"),
  };
  
  [JsonPropertyName("stairs")] public BuildingSelection[] stairs { get; }= {
    new BuildingSelection(1.0m, "stairs1"),
    new BuildingSelection(1.0m, "stairs2"),
    new BuildingSelection(1.0m, "stairsnormal"),
    new BuildingSelection(1.0m, "stairsbig"),
  };
  
  [JsonPropertyName("fountains")] public BuildingSelection[] fountains { get; }= {
    new BuildingSelection(1.0m, "fountain1"),
    new BuildingSelection(1.0m, "fountain2"),
    new BuildingSelection(1.0m, "fountain3"),
  };
  
  [JsonPropertyName("parks")] public BuildingSelection[] parks { get; }= {
    new BuildingSelection(1.0m, "park_pool"),
    new BuildingSelection(1.0m, "park_plants"),
    new BuildingSelection(1.0m, "park_trees"),
    new BuildingSelection(1.0m, "park_plants_pillars"),
    new BuildingSelection(1.0m, "park_fountain1"),
    new BuildingSelection(1.0m, "park_fountain2"),
    new BuildingSelection(1.0m, "park_building1"),
    new BuildingSelection(1.0m, "park_building2"),
  };
  
  [JsonPropertyName("raildungeons")] public BuildingSelection[] raildungeons { get; }= {
    new BuildingSelection(1.0m, "rail_dungeon1"),
    new BuildingSelection(1.0m, "rail_dungeon2"),
  };

}

public class BuildingSelection
{
  public BuildingSelection(decimal factor, string value)
  {
    this.factor = factor;
    this.value = value;
  }

  [JsonPropertyName("factor")] public decimal factor { get; }
  [JsonPropertyName("value")] public string value { get; }
}