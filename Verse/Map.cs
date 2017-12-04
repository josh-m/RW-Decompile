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
	public sealed class Map : IIncidentTarget, IThingHolder, IExposable, ILoadReferenceable
	{
		public MapFileCompressor compressor;

		private List<Thing> loadedFullThings;

		public int uniqueID = -1;

		public MapInfo info = new MapInfo();

		public List<MapComponent> components = new List<MapComponent>();

		public ThingOwner spawnedThings;

		public CellIndices cellIndices;

		public ListerThings listerThings;

		public ListerBuildings listerBuildings;

		public MapPawns mapPawns;

		public DynamicDrawManager dynamicDrawManager;

		public MapDrawer mapDrawer;

		public PawnDestinationReservationManager pawnDestinationReservationManager;

		public TooltipGiverList tooltipGiverList;

		public ReservationManager reservationManager;

		public PhysicalInteractionReservationManager physicalInteractionReservationManager;

		public DesignationManager designationManager;

		public LordManager lordManager;

		public PassingShipManager passingShipManager;

		public SlotGroupManager slotGroupManager;

		public DebugCellDrawer debugDrawer;

		public GameConditionManager gameConditionManager;

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

		public StoryState storyState;

		public RoadInfo roadInfo;

		public WaterInfo waterInfo;

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
				return this.info.parent.Faction;
			}
		}

		public int Area
		{
			get
			{
				return this.Size.x * this.Size.z;
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return this.info.parent;
			}
		}

		public IEnumerable<IntVec3> AllCells
		{
			get
			{
				for (int z = 0; z < this.Size.z; z++)
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
				return this.info.parent is FactionBase && this.info.parent.Faction == Faction.OfPlayer;
			}
		}

		public bool IsTempIncidentMap
		{
			get
			{
				return this.info.parent.def.isTempIncidentMapOwner;
			}
		}

		public int Tile
		{
			get
			{
				return this.info.Tile;
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

		public StoryState StoryState
		{
			get
			{
				return this.storyState;
			}
		}

		public GameConditionManager GameConditionManager
		{
			get
			{
				return this.gameConditionManager;
			}
		}

		public float PlayerWealthForStoryteller
		{
			get
			{
				if (this.IsPlayerHome)
				{
					return this.wealthWatcher.WealthItems + this.wealthWatcher.WealthBuildings * 0.5f;
				}
				float num = 0f;
				foreach (Pawn current in this.mapPawns.FreeColonists)
				{
					num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(current);
				}
				return num;
			}
		}

		public IEnumerable<Pawn> FreeColonistsForStoryteller
		{
			get
			{
				return this.mapPawns.FreeColonists;
			}
		}

		public FloatRange IncidentPointsRandomFactorRange
		{
			get
			{
				return FloatRange.One;
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

		public IEnumerable<IncidentTargetTypeDef> AcceptedTypes()
		{
			return this.info.parent.AcceptedTypes();
		}

		public void ConstructComponents()
		{
			this.spawnedThings = new ThingOwner<Thing>(this);
			this.cellIndices = new CellIndices(this);
			this.listerThings = new ListerThings(ListerThingsUse.Global);
			this.listerBuildings = new ListerBuildings();
			this.mapPawns = new MapPawns(this);
			this.dynamicDrawManager = new DynamicDrawManager(this);
			this.mapDrawer = new MapDrawer(this);
			this.tooltipGiverList = new TooltipGiverList();
			this.pawnDestinationReservationManager = new PawnDestinationReservationManager();
			this.reservationManager = new ReservationManager(this);
			this.physicalInteractionReservationManager = new PhysicalInteractionReservationManager();
			this.designationManager = new DesignationManager(this);
			this.lordManager = new LordManager(this);
			this.debugDrawer = new DebugCellDrawer();
			this.passingShipManager = new PassingShipManager(this);
			this.slotGroupManager = new SlotGroupManager(this);
			this.gameConditionManager = new GameConditionManager(this);
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
			this.storyState = new StoryState(this);
			this.components.Clear();
			this.FillComponents();
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", -1, false);
			Scribe_Deep.Look<MapInfo>(ref this.info, "mapInfo", new object[0]);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.compressor = new MapFileCompressor(this);
				this.compressor.BuildCompressedString();
				this.ExposeComponents();
				this.compressor.ExposeData();
				HashSet<string> hashSet = new HashSet<string>();
				if (Scribe.EnterNode("things"))
				{
					try
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
									Scribe_Deep.Look<Thing>(ref thing, "thing", new object[0]);
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
					}
					finally
					{
						Scribe.ExitNode();
					}
				}
				else
				{
					Log.Error("Could not enter the things node while saving.");
				}
				this.compressor = null;
			}
			else
			{
				if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					this.ConstructComponents();
					this.regionAndRoomUpdater.Enabled = false;
					this.compressor = new MapFileCompressor(this);
				}
				this.ExposeComponents();
				DeepProfiler.Start("Load compressed things");
				this.compressor.ExposeData();
				DeepProfiler.End();
				DeepProfiler.Start("Load non-compressed things");
				Scribe_Collections.Look<Thing>(ref this.loadedFullThings, "things", LookMode.Deep, new object[0]);
				DeepProfiler.End();
			}
		}

		private void FillComponents()
		{
			this.components.RemoveAll((MapComponent component) => component == null);
			foreach (Type current in typeof(MapComponent).AllSubclassesNonAbstract())
			{
				if (this.GetComponent(current) == null)
				{
					MapComponent item = (MapComponent)Activator.CreateInstance(current, new object[]
					{
						this
					});
					this.components.Add(item);
				}
			}
			this.roadInfo = this.GetComponent<RoadInfo>();
			this.waterInfo = this.GetComponent<WaterInfo>();
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
				if (!(current2 is Building))
				{
					try
					{
						GenSpawn.Spawn(current2, current2.Position, this, current2.Rotation, true);
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
			}
			foreach (Building current3 in from t in list2.OfType<Building>()
			orderby t.def.size.Magnitude
			select t)
			{
				try
				{
					GenSpawn.SpawnBuildingAsPossible(current3, this, true);
				}
				catch (Exception ex2)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception spawning loaded thing ",
						current3,
						": ",
						ex2
					}));
				}
			}
			DeepProfiler.End();
			this.FinalizeInit();
		}

		public void FinalizeInit()
		{
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
			MapComponentUtility.FinalizeInit(this);
		}

		private void ExposeComponents()
		{
			Scribe_Deep.Look<WeatherManager>(ref this.weatherManager, "weatherManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<ReservationManager>(ref this.reservationManager, "reservationManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<PhysicalInteractionReservationManager>(ref this.physicalInteractionReservationManager, "physicalInteractionReservationManager", new object[0]);
			Scribe_Deep.Look<DesignationManager>(ref this.designationManager, "designationManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<PawnDestinationReservationManager>(ref this.pawnDestinationReservationManager, "pawnDestinationReservationManager", new object[0]);
			Scribe_Deep.Look<LordManager>(ref this.lordManager, "lordManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<PassingShipManager>(ref this.passingShipManager, "visitorManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<GameConditionManager>(ref this.gameConditionManager, "gameConditionManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<FogGrid>(ref this.fogGrid, "fogGrid", new object[]
			{
				this
			});
			Scribe_Deep.Look<RoofGrid>(ref this.roofGrid, "roofGrid", new object[]
			{
				this
			});
			Scribe_Deep.Look<TerrainGrid>(ref this.terrainGrid, "terrainGrid", new object[]
			{
				this
			});
			Scribe_Deep.Look<ZoneManager>(ref this.zoneManager, "zoneManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<TemperatureCache>(ref this.temperatureCache, "temperatureCache", new object[]
			{
				this
			});
			Scribe_Deep.Look<SnowGrid>(ref this.snowGrid, "snowGrid", new object[]
			{
				this
			});
			Scribe_Deep.Look<AreaManager>(ref this.areaManager, "areaManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<VoluntarilyJoinableLordsStarter>(ref this.lordsStarter, "lordsStarter", new object[]
			{
				this
			});
			Scribe_Deep.Look<AttackTargetReservationManager>(ref this.attackTargetReservationManager, "attackTargetReservationManager", new object[]
			{
				this
			});
			Scribe_Deep.Look<DeepResourceGrid>(ref this.deepResourceGrid, "deepResourceGrid", new object[]
			{
				this
			});
			Scribe_Deep.Look<WeatherDecider>(ref this.weatherDecider, "weatherDecider", new object[]
			{
				this
			});
			Scribe_Deep.Look<DamageWatcher>(ref this.damageWatcher, "damageWatcher", new object[0]);
			Scribe_Deep.Look<RememberedCameraPos>(ref this.rememberedCameraPos, "rememberedCameraPos", new object[]
			{
				this
			});
			Scribe_Deep.Look<MineStrikeManager>(ref this.mineStrikeManager, "mineStrikeManager", new object[0]);
			Scribe_Deep.Look<StoryState>(ref this.storyState, "storyState", new object[]
			{
				this
			});
			Scribe_Collections.Look<MapComponent>(ref this.components, "components", LookMode.Deep, new object[]
			{
				this
			});
			this.FillComponents();
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.MapPostLoadInit(this);
			}
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
				this.gameConditionManager.GameConditionManagerTick();
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
			MapComponentUtility.MapComponentTick(this);
		}

		public void MapUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			this.skyManager.SkyManagerUpdate();
			this.powerNetManager.UpdatePowerNetsAndConnections_First();
			this.regionGrid.UpdateClean();
			this.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
			this.glowGrid.GlowGridUpdate_First();
			this.lordManager.LordManagerUpdate();
			if (!worldRenderedNow && Find.VisibleMap == this)
			{
				this.waterInfo.SetTextures();
				Find.FactionManager.FactionsDebugDrawOnMap();
				this.mapDrawer.MapMeshDrawerUpdate_First();
				this.powerNetGrid.DrawDebugPowerNetGrid();
				DoorsDebugDrawer.DrawDebug();
				this.mapDrawer.DrawMapMesh();
				this.dynamicDrawManager.DrawDynamicThings();
				this.gameConditionManager.GameConditionManagerDraw();
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
			MapComponentUtility.MapComponentUpdate(this);
		}

		public T GetComponent<T>() where T : MapComponent
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				T t = this.components[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		public MapComponent GetComponent(Type type)
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				if (type.IsAssignableFrom(this.components[i].GetType()))
				{
					return this.components[i];
				}
			}
			return null;
		}

		public string GetUniqueLoadID()
		{
			return "Map_" + this.uniqueID;
		}

		public override string ToString()
		{
			string str = "(Map-" + this.uniqueID;
			if (this.IsPlayerHome)
			{
				str += "-PlayerHome";
			}
			return str + ")";
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.spawnedThings;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			this.GetNonThingChildHolders(outChildren);
		}

		public void GetNonThingChildHolders(List<IThingHolder> outChildren)
		{
			List<PassingShip> passingShips = this.passingShipManager.passingShips;
			for (int i = 0; i < passingShips.Count; i++)
			{
				IThingHolder thingHolder = passingShips[i] as IThingHolder;
				if (thingHolder != null)
				{
					outChildren.Add(thingHolder);
				}
			}
			for (int j = 0; j < this.components.Count; j++)
			{
				IThingHolder thingHolder2 = this.components[j] as IThingHolder;
				if (thingHolder2 != null)
				{
					outChildren.Add(thingHolder2);
				}
			}
		}
	}
}
