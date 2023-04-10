using Newtonsoft.Json;

namespace schematic_to_lost_cities.LostCities;

public class Part
{
	private PartFloor[] _floors;
	public string Name;

	public Part(Schematic.Schematic schematic, Palette palette)
	{
		Name = schematic.Name;
		var height = schematic.Blocks.GetLength(0);
		var floors = (height / PartFloor.SLICES_PER_FLOOR ) + 1;
		_floors = new PartFloor[floors];

		for (var i = 0; i < floors; i++)
		{
			_floors[i] = new PartFloor();
			for (var j = 0; j < PartFloor.SLICES_PER_FLOOR; j++)
			{
			 _floors[i]
				 .AddLayer(j, schematic.Blocks, (i * PartFloor.SLICES_PER_FLOOR) + j, palette);
			}
		}
	}

	public void Serialize()
	{
		var partsDir = "output/data/lostcities/lostcities/parts";
		if (!Directory.Exists(partsDir))
		{
			Directory.CreateDirectory(partsDir);
		}

		var buildingsDir = "output/data/lostcities/lostcities/buildings";
		if (!Directory.Exists(buildingsDir))
		{
			Directory.CreateDirectory(buildingsDir);
		}

		var partList = new List<BuildingPart>();
		

		for (var index = 0; index < _floors.Length; index++)
		{
			var floor = _floors[index];
			var part = $"{Name}_{index}";
			File.WriteAllText($"{partsDir}/{part}.json", JsonConvert.SerializeObject(floor, Formatting.Indented));
			
			partList.Add(new BuildingPart(){part = part, top = index == _floors.Length -1});
		}

		var building = new Building()
		{
			parts = partList.ToArray()
		};
		
		File.WriteAllText($"{buildingsDir}/{Name}.json", JsonConvert.SerializeObject(building, Formatting.Indented));
	}
}

public class Building
{
	public string filler = "#";
	public string rubble = "}";
	public BuildingPart[] parts { get; set; }
}

public class BuildingPart
{
	public string part { get; set; }
	public bool top { get; set; }
}
