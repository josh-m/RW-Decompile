using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public sealed class Map : IIncidentTarget, ILoadReferenceable, IExposable
	{
		public MapFileCompressor compressor;

		private List<Thing> loadedFullThings;

		public int uniqueID = -1;

		public MapInfo info = new MapInfo();

		public List<MapComponent> components = new List<MapComponent>();

		public CellIndices cellIndices;

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

		public DebugCellDrawer debugDrawer;

		public MapConditionManager mapConditionManager;

		public WeatherManager weatherManager;

		public ZoneManager zoneManager;

		public ResourceCounter resourceCounter;

		public MapTemperature mapTemperature;

		public TemperatureCache temperatureCache;

		public AreaManager areaManager;

		public AttackTargetsCache attackTargetsCache;

		public AttackTargetReservationManager attackTargetReservationManager;

		public VoluntarilyJoinableLordsStarter lordsStarter;

		public ThingGrid thingGrid;

		public CoverGrid coverGrid;

		public EdificeGrid edificeGrid;

		public FogGrid fogGrid;

		public RegionGrid regionGrid;

		public GlowGrid glowGrid;

		public TerrainGrid terrainGrid;

		public PathGrid pathGrid;

		public RoofGrid roofGrid;

		public FertilityGrid fertilityGrid;

		public SnowGrid snowGrid;

		public DeepResourceGrid deepResourceGrid;

		public ExitMapGrid exitMapGrid;

		public LinkGrid linkGrid;

		public GlowFlooder glowFlooder;

		public PowerNetManager powerNetManager;

		public PowerNetGrid powerNetGrid;

		public RegionMaker regionMaker;

		public PathFinder pathFinder;

		public PawnPathPool pawnPathPool;

		public RegionAndRoomUpdater regionAndRoomUpdater;

		public RegionLinkDatabase regionLinkDatabase;

		public MoteCounter moteCounter;

		public GatherSpotLister gatherSpotLister;

		public WindManager windManager;

		public ListerBuildingsRepairable listerBuildingsRepairable;

		public ListerHaulables listerHaulables;

		public ListerFilthInHomeArea listerFilthInHomeArea;

		public Reachability reachability;

		public ItemAvailability itemAvailability;

		public AutoBuildRoofAreaSetter autoBuildRoofAreaSetter;

		public RoofCollapseBufferResolver roofCollapseBufferResolver;

		public RoofCollapseBuffer roofCollapseBuffer;

		public WildSpawner wildSpawner;

		public SteadyAtmosphereEffects steadyAtmosphereEffects;

		public SkyManager skyManager;

		public OverlayDrawer overlayDrawer;

		public FloodFiller floodFiller;

		public WeatherDecider weatherDecider;

		public FireWatcher fireWatcher;

		public DangerWatcher dangerWatcher;

		public DamageWatcher damageWatcher;

		public StrengthWatcher strengthWatcher;

		public WealthWatcher wealthWatcher;

		public RegionDirtyer regionDirtyer;

		public MapCellsInRandomOrder cellsInRandomOrder;

		public RememberedCameraPos rememberedCameraPos;

		public MineStrikeManager mineStrikeManager;

		public int Index
		{
			get
			{
				return Find.Maps.IndexOf(this);
			}
		}

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

		public Faction ParentFaction
		{
			get
			{
				if (this.info.parent != null)
				{
					return this.info.parent.Faction;
				}
				return null;
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

		public bool IsPlayerHome
		{
			get
			{
				return this.info.parent != null && this.info.parent is FactionBase && this.info.parent.Faction == Faction.OfPlayer;
			}
		}

		public int Tile
		{
			get
			{
				return this.info.tile;
			}
		}

		public Tile TileInfo
		{
			get
			{
				return Find.WorldGrid[this.Tile];
			}
		}

		public BiomeDef Biome
		{
			get
			{
				return this.TileInfo.biome;
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
			this.cellIndices = new CellIndices(this);
			this.listerThings = new ListerThings(ListerThingsUse.Global);
			this.listerBuildings = new ListerBuildings();
			this.mapPawns = new MapPawns(this);
			this.dynamicDrawManager = new DynamicDrawManager(this);
			this.mapDrawer = new MapDrawer(this);
			this.tooltipGiverList = new TooltipGiverList();
			this.pawnDestinationManager = new PawnDestinationManager();
			this.reservationManager = new ReservationManager(this);
			this.physicalInteractionReservationManager = new PhysicalInteractionReservationManager();
			this.designationManager = new DesignationManager(this);
			this.lordManager = new LordManager(this);
			this.debugDrawer = new DebugCellDrawer();
			this.passingShipManager = new PassingShipManager(this);
			this.slotGroupManager = new SlotGroupManager(this);
			this.mapConditionManager = new MapConditionManager(this);
			this.weatherManager = new WeatherManager(this);
			this.zoneManager = new ZoneManager(this);
			this.resourceCounter = new ResourceCounter(this);
			this.mapTemperature = new MapTemperature(this);
			this.temperatureCache = new TemperatureCache(this);
			this.areaManager = new AreaManager(this);
			this.attackTargetsCache = new AttackTargetsCache(this);
			this.attackTargetReservationManager = new AttackTargetReservationManager(this);
			this.lordsStarter = new VoluntarilyJoinableLordsStarter(this);
			this.thingGrid = new ThingGrid(this);
			this.coverGrid = new CoverGrid(this);
			this.edificeGrid = new EdificeGrid(this);
			this.fogGrid = new FogGrid(this);
			this.glowGrid = new GlowGrid(this);
			this.regionGrid = new RegionGrid(this);
			this.terrainGrid = new TerrainGrid(this);
			this.pathGrid = new PathGrid(this);
			this.roofGrid = new RoofGrid(this);
			this.fertilityGrid = new FertilityGrid(this);
			this.snowGrid = new SnowGrid(this);
			this.deepResourceGrid = new DeepResourceGrid(this);
			this.exitMapGrid = new ExitMapGrid(this);
			this.linkGrid = new LinkGrid(this);
			this.glowFlooder = new GlowFlooder(this);
			this.powerNetManager = new PowerNetManager(this);
			this.powerNetGrid = new PowerNetGrid(this);
			this.regionMaker = new RegionMaker(this);
			this.pathFinder = new PathFinder(this);
			this.pawnPathPool = new PawnPathPool(this);
			this.regionAndRoomUpdater = new RegionAndRoomUpdater(this);
			this.regionLinkDatabase = new RegionLinkDatabase();
			this.moteCounter = new MoteCounter();
			this.gatherSpotLister = new GatherSpotLister();
			this.windManager = new WindManager(this);
			this.listerBuildingsRepairable = new ListerBuildingsRepairable();
			this.listerHaulables = new ListerHaulables(this);
			this.listerFilthInHomeArea = new ListerFilthInHomeArea(this);
			this.reachability = new Reachability(this);
			this.itemAvailability = new ItemAvailability(this);
			this.autoBuildRoofAreaSetter = new AutoBuildRoofAreaSetter(this);
			this.roofCollapseBufferResolver = new RoofCollapseBufferResolver(this);
			this.roofCollapseBuffer = new RoofCollapseBuffer();
			this.wildSpawner = new WildSpawner(this);
			this.steadyAtmosphereEffects = new SteadyAtmosphereEffects(this);
			this.skyManager = new SkyManager(this);
			this.overlayDrawer = new OverlayDrawer();
			this.floodFiller = new FloodFiller(this);
			this.weatherDecider = new WeatherDecider(this);
			this.fireWatcher = new FireWatcher(this);
			this.dangerWatcher = new DangerWatcher(this);
			this.damageWatcher = new DamageWatcher();
			this.strengthWatcher = new StrengthWatcher(this);
			this.wealthWatcher = new WealthWatcher(this);
			this.regionDirtyer = new RegionDirtyer(this);
			this.cellsInRandomOrder = new MapCellsInRandomOrder(this);
			this.rememberedCameraPos = new RememberedCameraPos(this);
			this.mineStrikeManager = new MineStrikeManager();
			foreach (Type current in typeof(MapComponent).AllLeafSubclasses())
			{
				MapComponent item = (MapComponent)Activator.CreateInstance(current, new object[]
				{
					this
				});
				this.components.Add(item);
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.uniqueID, "uniqueID", -1, false);
			Scribe_Deep.LookDeep<MapInfo>(ref this.info, "mapInfo", new object[0]);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.compressor = new MapFileCompressor(this);
				this.compressor.BuildCompressedString();
				this.ExposeComponents();
				this.compressor.ExposeData();
				HashSet<string> hashSet = new HashSet<string>();
				if (Scribe.EnterNode("things"))
				{
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
				this.compressor = null;
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.ConstructComponents();
				this.regionAndRoomUpdater.Enabled = false;
				this.ExposeComponents();
				DeepProfiler.Start("Load compressed things");
				this.compressor = new MapFileCompressor(this);
				this.compressor.ExposeData();
				DeepProfiler.End();
				DeepProfiler.Start("Load non-compressed things");
				Scribe_Collections.LookList<Thing>(ref this.loadedFullThings, "things", LookMode.Deep, new object[0]);
				DeepProfiler.End();
			}
		}

		public void FinalizeLoading()
		{
			List<Thing> list = this.compressor.ThingsToSpawnAfterLoad().ToList<Thing>();
			this.compressor = null;
			DeepProfiler.Start("Merge compressed and non-compressed thing lists");
			List<Thing> list2 = new List<Thing>(this.loadedFullThings.Count + list.Count);
			foreach (Thing current in this.loadedFullThings.Concat(list))
			{
				list2.Add(current);
			}
			this.loadedFullThings.Clear();
			DeepProfiler.End();
			DeepProfiler.Start("Spawn everything into the map");
			foreach (Thing current2 in list2)
			{
				try
				{
					GenSpawn.Spawn(current2, current2.Position, this, current2.Rotation);
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception spawning loaded thing ",
						current2,
						": ",
						ex
					}));
				}
			}
			DeepProfiler.End();
			this.FinalizeInit();
		}

		public void FinalizeInit()
		{
			this.mapTemperature.UpdateCachedData();
			this.pathGrid.RecalculateAllPerceivedPathCosts();
			this.regionAndRoomUpdater.Enabled = true;
			this.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			this.powerNetManager.UpdatePowerNetsAndConnections_First();
			this.temperatureCache.temperatureSaveLoad.ApplyLoadedDataToRegions();
			foreach (Thing current in this.listerThings.AllThings.ToList<Thing>())
			{
				try
				{
					current.PostMapInit();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception PostMapInit in ",
						current,
						": ",
						ex
					}));
				}
			}
			this.listerFilthInHomeArea.RebuildAll();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.mapDrawer.RegenerateEverythingNow();
			});
			this.resourceCounter.UpdateResourceCounts();
			this.wealthWatcher.ForceRecount(true);
		}

		private void ExposeComponents()
		{
			Scribe_Deep.LookDeep<WeatherManager>(ref this.weatherManager, "weatherManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<ReservationManager>(ref this.reservationManager, "reservationManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<PhysicalInteractionReservationManager>(ref this.physicalInteractionReservationManager, "physicalInteractionReservationManager", new object[0]);
			Scribe_Deep.LookDeep<DesignationManager>(ref this.designationManager, "designationManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<LordManager>(ref this.lordManager, "lordManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<PassingShipManager>(ref this.passingShipManager, "visitorManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<MapConditionManager>(ref this.mapConditionManager, "mapConditionManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<FogGrid>(ref this.fogGrid, "fogGrid", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<RoofGrid>(ref this.roofGrid, "roofGrid", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<TerrainGrid>(ref this.terrainGrid, "terrainGrid", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<ZoneManager>(ref this.zoneManager, "zoneManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<TemperatureCache>(ref this.temperatureCache, "temperatureCache", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<SnowGrid>(ref this.snowGrid, "snowGrid", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<AreaManager>(ref this.areaManager, "areaManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<VoluntarilyJoinableLordsStarter>(ref this.lordsStarter, "lordsStarter", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<AttackTargetReservationManager>(ref this.attackTargetReservationManager, "attackTargetReservationManager", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<DeepResourceGrid>(ref this.deepResourceGrid, "deepResourceGrid", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<WeatherDecider>(ref this.weatherDecider, "weatherDecider", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<DamageWatcher>(ref this.damageWatcher, "damageWatcher", new object[0]);
			Scribe_Deep.LookDeep<RememberedCameraPos>(ref this.rememberedCameraPos, "rememberedCameraPos", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<MineStrikeManager>(ref this.mineStrikeManager, "mineStrikeManager", new object[0]);
			Scribe_Collections.LookList<MapComponent>(ref this.components, "components", LookMode.Deep, new object[]
			{
				this
			});
		}

		public void MapPreTick()
		{
			this.itemAvailability.Tick();
			this.listerHaulables.ListerHaulablesTick();
			try
			{
				this.autoBuildRoofAreaSetter.AutoBuildRoofAreaSetterTick_First();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			this.roofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
			this.windManager.WindManagerTick();
			try
			{
				this.mapTemperature.MapTemperatureTick();
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
		}

		public void MapPostTick()
		{
			try
			{
				this.wildSpawner.WildSpawnerTick();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			try
			{
				this.powerNetManager.PowerNetsTick();
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
			try
			{
				this.steadyAtmosphereEffects.SteadyAtmosphereEffectsTick();
			}
			catch (Exception ex3)
			{
				Log.Error(ex3.ToString());
			}
			try
			{
				this.lordManager.LordManagerTick();
			}
			catch (Exception ex4)
			{
				Log.Error(ex4.ToString());
			}
			try
			{
				this.passingShipManager.PassingShipManagerTick();
			}
			catch (Exception ex5)
			{
				Log.Error(ex5.ToString());
			}
			try
			{
				this.debugDrawer.DebugDrawerTick();
			}
			catch (Exception ex6)
			{
				Log.Error(ex6.ToString());
			}
			try
			{
				this.lordsStarter.VoluntarilyJoinableLordsStarterTick();
			}
			catch (Exception ex7)
			{
				Log.Error(ex7.ToString());
			}
			try
			{
				this.mapConditionManager.MapConditionManagerTick();
			}
			catch (Exception ex8)
			{
				Log.Error(ex8.ToString());
			}
			try
			{
				this.weatherManager.WeatherManagerTick();
			}
			catch (Exception ex9)
			{
				Log.Error(ex9.ToString());
			}
			try
			{
				this.resourceCounter.ResourceCounterTick();
			}
			catch (Exception ex10)
			{
				Log.Error(ex10.ToString());
			}
			try
			{
				this.weatherDecider.WeatherDeciderTick();
			}
			catch (Exception ex11)
			{
				Log.Error(ex11.ToString());
			}
			try
			{
				this.fireWatcher.FireWatcherTick();
			}
			catch (Exception ex12)
			{
				Log.Error(ex12.ToString());
			}
			try
			{
				this.damageWatcher.DamageWatcherTick();
			}
			catch (Exception ex13)
			{
				Log.Error(ex13.ToString());
			}
			for (int i = 0; i < this.components.Count; i++)
			{
				try
				{
					this.components[i].MapComponentTick();
				}
				catch (Exception ex14)
				{
					Log.Error(ex14.ToString());
				}
			}
		}

		public void MapUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			this.skyManager.SkyManagerUpdate();
			this.powerNetManager.UpdatePowerNetsAndConnections_First();
			this.regionGrid.UpdateClean();
			this.regionAndRoomUpdater.RebuildDirtyRegionsAndRooms();
			this.glowGrid.GlowGridUpdate_First();
			this.lordManager.LordManagerUpdate();
			if (!worldRenderedNow && Find.VisibleMap == this)
			{
				Find.FactionManager.FactionsDebugDrawOnMap();
				this.mapDrawer.MapMeshDrawerUpdate_First();
				this.powerNetGrid.DrawDebugPowerNetGrid();
				this.mapDrawer.DrawMapMesh();
				this.dynamicDrawManager.DrawDynamicThings();
				this.mapConditionManager.MapConditionManagerDraw();
				MapEdgeClipDrawer.DrawClippers(this);
				this.designationManager.DrawDesignations();
				this.overlayDrawer.DrawAllOverlays();
			}
			try
			{
				this.areaManager.AreaManagerUpdate();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			this.weatherManager.WeatherManagerUpdate();
			this.deepResourceGrid.DeepResourceGridDraw(false);
			for (int i = 0; i < this.components.Count; i++)
			{
				this.components[i].MapComponentUpdate();
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

		public string GetUniqueLoadID()
		{
			return "Map_" + this.uniqueID;
		}

		public override string ToString()
		{
			string text = string.Concat(new object[]
			{
				"Map ",
				this.Index,
				" (ID ",
				this.uniqueID,
				")"
			});
			if (this.IsPlayerHome)
			{
				text += " (player home)";
			}
			if (this == Find.VisibleMap)
			{
				text += " (current)";
			}
			return text;
		}
	}
}
