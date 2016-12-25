using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public sealed class Map : IExposable
	{
		public MapInfo info = new MapInfo();

		public List<MapComponent> components = new List<MapComponent>();

		public ListerThings listerThings;

		public ListerBuildings listerBuildings;

		public MapPawns mapPawns;

		public DynamicDrawManager dynamicDrawManager;

		public MapDrawer mapDrawer;

		public PawnDestinationManager pawnDestinationManager;

		public TooltipGiverList tooltipGiverList;

		public ReservationManager reservationManager;

		public PhysicalInteractionReservationManager physicalInteractionReservationManager;

		public DesignationManager designationManager;

		public LordManager lordManager;

		public PassingShipManager passingShipManager;

		public SlotGroupManager slotGroupManager;

		public Autosaver autosaver;

		public DebugCellDrawer debugDrawer;

		public MapConditionManager mapConditionManager;

		public WeatherManager weatherManager;

		public ZoneManager zoneManager;

		public ResourceCounter resourceCounter;

		public MapAmbientSound ambientSound;

		public MusicManagerMap musicManagerMap;

		public TemperatureCache temperatureCache;

		public AreaManager areaManager;

		public AttackTargetsCache attackTargetsCache;

		public AttackTargetReservationManager attackTargetReservationManager;

		public VoluntarilyJoinableLordsStarter lordsStarter;

		public ThingGrid thingGrid;

		public CoverGrid coverGrid;

		public EdificeGrid buildingGrid;

		public FogGrid fogGrid;

		public RegionGrid regionGrid;

		public GlowGrid glowGrid;

		public TerrainGrid terrainGrid;

		public PathGrid pathGrid;

		public RoofGrid roofGrid;

		public FertilityGrid fertilityGrid;

		public SnowGrid snowGrid;

		public DeepResourceGrid deepResourceGrid;

		public IntVec3 Size
		{
			get
			{
				return this.info.Size;
			}
		}

		public IntVec3 Center
		{
			get
			{
				return new IntVec3(this.Size.x / 2, 0, this.Size.z / 2);
			}
		}

		public int Area
		{
			get
			{
				return this.Size.x * this.Size.z;
			}
		}

		public IEnumerable<IntVec3> AllCells
		{
			get
			{
				for (int z = 0; z < this.Size.x; z++)
				{
					for (int y = 0; y < this.Size.y; y++)
					{
						for (int x = 0; x < this.Size.x; x++)
						{
							yield return new IntVec3(x, y, z);
						}
					}
				}
			}
		}

		public WorldSquare WorldSquare
		{
			get
			{
				return Find.World.grid.Get(this.info.worldCoords);
			}
		}

		public BiomeDef Biome
		{
			get
			{
				return this.WorldSquare.biome;
			}
		}

		public IntVec2 WorldCoords
		{
			get
			{
				return this.info.worldCoords;
			}
		}

		[DebuggerHidden]
		public IEnumerator<IntVec3> GetEnumerator()
		{
			foreach (IntVec3 c in this.AllCells)
			{
				yield return c;
			}
		}

		public void ConstructComponents()
		{
			this.listerThings = new ListerThings(ListerThingsUse.Global);
			this.listerBuildings = new ListerBuildings();
			this.mapPawns = new MapPawns();
			this.dynamicDrawManager = new DynamicDrawManager();
			this.mapDrawer = new MapDrawer();
			this.tooltipGiverList = new TooltipGiverList();
			this.pawnDestinationManager = new PawnDestinationManager();
			this.ambientSound = new MapAmbientSound();
			this.reservationManager = new ReservationManager();
			this.physicalInteractionReservationManager = new PhysicalInteractionReservationManager();
			this.designationManager = new DesignationManager();
			this.lordManager = new LordManager();
			this.debugDrawer = new DebugCellDrawer();
			this.passingShipManager = new PassingShipManager();
			this.autosaver = new Autosaver();
			this.slotGroupManager = new SlotGroupManager();
			this.mapConditionManager = new MapConditionManager();
			this.weatherManager = new WeatherManager();
			this.zoneManager = new ZoneManager();
			this.musicManagerMap = new MusicManagerMap();
			this.resourceCounter = new ResourceCounter();
			this.temperatureCache = new TemperatureCache();
			this.areaManager = new AreaManager();
			this.attackTargetsCache = new AttackTargetsCache();
			this.attackTargetReservationManager = new AttackTargetReservationManager();
			this.lordsStarter = new VoluntarilyJoinableLordsStarter();
			this.thingGrid = new ThingGrid();
			this.coverGrid = new CoverGrid();
			this.buildingGrid = new EdificeGrid();
			this.fogGrid = new FogGrid();
			this.glowGrid = new GlowGrid();
			this.regionGrid = new RegionGrid();
			this.terrainGrid = new TerrainGrid();
			this.pathGrid = new PathGrid();
			this.roofGrid = new RoofGrid();
			this.fertilityGrid = new FertilityGrid();
			this.snowGrid = new SnowGrid();
			this.deepResourceGrid = new DeepResourceGrid();
			foreach (Type current in typeof(MapComponent).AllLeafSubclasses())
			{
				MapComponent item = (MapComponent)Activator.CreateInstance(current);
				this.components.Add(item);
			}
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Scribe_Deep.LookDeep<MapInfo>(ref this.info, "mapInfo", new object[0]);
				MapFileCompressor mapFileCompressor = new MapFileCompressor();
				mapFileCompressor.ReadDataFromMap();
				Find.Map.ExposeComponents();
				mapFileCompressor.ExposeData();
				HashSet<string> hashSet = new HashSet<string>();
				Scribe.EnterNode("things");
				foreach (Thing current in this.listerThings.AllThings)
				{
					try
					{
						if (current.def.isSaveable && !current.IsSaveCompressible())
						{
							if (hashSet.Contains(current.ThingID))
							{
								Log.Error("Saving Thing with already-used ID " + current.ThingID);
							}
							else
							{
								hashSet.Add(current.ThingID);
							}
							Thing thing = current;
							Scribe_Deep.LookDeep<Thing>(ref thing, "thing", new object[0]);
						}
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Exception saving ",
							current,
							": ",
							ex
						}));
					}
				}
				Scribe.ExitNode();
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Log.Error("To load a map, use MapIniter_LoadFromFile, not Map.ExposeData.");
			}
		}

		public void ExposeComponents()
		{
			Scribe_Deep.LookDeep<WeatherManager>(ref this.weatherManager, "weatherManager", new object[0]);
			Scribe_Deep.LookDeep<ReservationManager>(ref this.reservationManager, "reservationManager", new object[0]);
			Scribe_Deep.LookDeep<PhysicalInteractionReservationManager>(ref this.physicalInteractionReservationManager, "physicalInteractionReservationManager", new object[0]);
			Scribe_Deep.LookDeep<DesignationManager>(ref this.designationManager, "designationManager", new object[0]);
			Scribe_Deep.LookDeep<LordManager>(ref this.lordManager, "lordManager", new object[0]);
			Scribe_Deep.LookDeep<PassingShipManager>(ref this.passingShipManager, "visitorManager", new object[0]);
			Scribe_Deep.LookDeep<MapConditionManager>(ref this.mapConditionManager, "mapConditionManager", new object[0]);
			Scribe_Deep.LookDeep<FogGrid>(ref this.fogGrid, "fogGrid", new object[0]);
			Scribe_Deep.LookDeep<RoofGrid>(ref this.roofGrid, "roofGrid", new object[0]);
			Scribe_Deep.LookDeep<TerrainGrid>(ref this.terrainGrid, "terrainGrid", new object[0]);
			Scribe_Deep.LookDeep<ZoneManager>(ref this.zoneManager, "zoneManager", new object[0]);
			Scribe_Deep.LookDeep<TemperatureCache>(ref this.temperatureCache, "temperatureGrid", new object[0]);
			Scribe_Deep.LookDeep<SnowGrid>(ref this.snowGrid, "snowGrid", new object[0]);
			Scribe_Deep.LookDeep<AreaManager>(ref this.areaManager, "areaManager", new object[0]);
			Scribe_Deep.LookDeep<VoluntarilyJoinableLordsStarter>(ref this.lordsStarter, "lordsStarter", new object[0]);
			Scribe_Deep.LookDeep<AttackTargetReservationManager>(ref this.attackTargetReservationManager, "attackTargetReservationManager", new object[0]);
			Scribe_Deep.LookDeep<DeepResourceGrid>(ref this.deepResourceGrid, "deepResourceGrid", new object[0]);
			Scribe_Collections.LookList<MapComponent>(ref this.components, "components", LookMode.Deep, new object[0]);
			Find.CameraDriver.Expose();
		}

		public void MapUpdate()
		{
			PowerNetManager.UpdatePowerNetsAndConnections_First();
			PowerNetGrid.DrawDebugPowerNetGrid();
			this.regionGrid.UpdateClean();
			RegionAndRoomUpdater.RebuildDirtyRegionsAndRooms();
			this.glowGrid.GlowGridUpdate_First();
			this.lordManager.LordManagerUpdate();
			Find.FactionManager.FactionManagerUpdate();
			this.mapDrawer.MapMeshDrawerUpdate_First();
			this.mapDrawer.DrawMapMesh();
			this.dynamicDrawManager.DrawDynamicThings();
			this.mapConditionManager.MapConditionManagerDraw();
			MapEdgeClipDrawer.DrawClippers();
			this.designationManager.DrawDesignations();
			try
			{
				Find.AreaManager.AreaManagerUpdate();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			OverlayDrawer.DrawAllOverlays();
			this.musicManagerMap.MusicUpdate();
			this.weatherManager.WeatherManagerUpdate();
			this.deepResourceGrid.DeepResourceGridDraw(false);
			for (int i = 0; i < this.components.Count; i++)
			{
				this.components[i].MapComponentUpdate();
			}
		}

		public void MapCleanup()
		{
			foreach (Thing current in this.listerThings.AllThings.ToList<Thing>())
			{
				current.Destroy(DestroyMode.Vanish);
			}
			AudioSource[] array = Find.Camera.GetComponents<AudioSource>();
			for (int i = 0; i < array.Length; i++)
			{
				AudioSource obj = array[i];
				UnityEngine.Object.Destroy(obj);
			}
		}

		public T GetComponent<T>() where T : MapComponent
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				if (this.components[i] is T)
				{
					return (T)((object)this.components[i]);
				}
			}
			return (T)((object)null);
		}
	}
}
