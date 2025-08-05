using Jab;
using Minecraft.City.Datapack.Generator.Builder;
using Minecraft.City.Datapack.Generator.Builder.Buildings;
using Minecraft.City.Datapack.Generator.Builder.Roads;
using Minecraft.City.Datapack.Generator.Models.PackMetadata;
using Minecraft.City.Datapack.Generator.Writers;
using Minecraft.City.Datapack.Generator.Writers.StaticWriters;

namespace Minecraft.City.Datapack.Generator;

[ServiceProvider]
[Transient<IStaticWriter, ForgeModStaticWriter>]
[Transient<PackMetadata, PackMetadata>]
[Transient<JsonWriter, JsonWriter>]
[Transient<IAssembler, RoadAssembler>]
[Transient<IAssembler, BuildingAssembler>]
[Transient<JarWriter, JarWriter>]
[Singleton<IBuildingZoneService, BuildingZoneService>]
public partial class Dependencies;
