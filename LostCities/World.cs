using Newtonsoft.Json;

namespace schematic_to_lost_cities.LostCities;

public class World
{
  public string outsidestyle = "outside";
  public Scattered scattered = new Scattered(new ScatteredList[]
  {
    new ScatteredList()
    {
      name = "radiotower",
      weight = 15,
      maxheightdiff = 3,
      biomes = new BiomesList()
      {
        excluding = new []
        {
          "#minecraft:is_ocean",
          "#minecraft:is_river",
          "#minecraft:is_beach",
        }
      }
    },
    new ScatteredList()
    {
      name = "oilrig",
      weight = 1,
      maxheightdiff = 100,
      biomes = new BiomesList()
      {
        if_any = new []
        {
          "#minecraft:is_deep_ocean"
        }
      }
    },
    new ScatteredList()
    {
      name = "cabin",
      weight = 10,
      maxheightdiff = 2,
      biomes = new BiomesList()
      {
        excluding = new []
        {
          "#minecraft:is_ocean",
          "#minecraft:is_river",
          "#minecraft:is_beach",
        }
      }
    }
  });

  public CityBiomeMultipliers[] citybiomemultipliers = new CityBiomeMultipliers[]
  {
    new CityBiomeMultipliers()
    {
      multiplier = .1m,
      biomes = new BiomesList()
      {
        if_any = new[]
        {
          "#minecraft:is_ocean"
        }
      }
    },
    new CityBiomeMultipliers()
    {
      multiplier = 0.3m,
      biomes = new BiomesList()
      {
        if_any = new []
        {
          "#minecraft:is_river"
        }
      }
    }
  };

  public CityStyles[] citystyles = new[]
  {
    new CityStyles()
    {
      factor = 1,
      citystyle = "schematic-assets:schematic-style"
    }
  };
  
	public void Serialize()
	{
		var worldDir = "output/data/schematic-assets/lostcities/worldstyles";
		if (!Directory.Exists(worldDir))
		{
			Directory.CreateDirectory(worldDir);
		}
		File.WriteAllText($"{worldDir}/schematic-world.json", Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings()
    {
      NullValueHandling = NullValueHandling.Ignore
    }));
	}
}

public class CityStyles
{
  public decimal factor { get; set; }
  public string citystyle { get; set; }
}

public class CityBiomeMultipliers
{
  public decimal multiplier { get; set; }
  public BiomesList biomes { get; set; }
}

public class Scattered
{
  public int areasize = 8;
  public decimal chance = 0.3m;
  public int weightnone = 20;
  public ScatteredList[] list { get; }

  public Scattered(ScatteredList[] scatteredlist)
  {
    this.list = scatteredlist;
  }

}

public class ScatteredList
{
  public string name { get; set; }
  public int weight { get; set; }
  public int maxheightdiff { get; set; }
  public BiomesList biomes { get; set; }
}

public class BiomesList
{
  public string[] excluding { get; set; }
  public string[] if_any { get; set; }
}