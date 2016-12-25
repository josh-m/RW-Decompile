using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public sealed class World : IExposable
	{
		public WorldInfo info = new WorldInfo();

		public WorldGrid grid;

		public WorldPathGrid pathGrid;

		public WorldRenderer renderer;

		public WorldObjectsHolder worldObjects;

		public WorldInterface UI;

		public WorldDebugDrawer debugDrawer;

		public WorldDynamicDrawManager dynamicDrawManager;

		public WorldPathFinder pathFinder;

		public WorldPathPool pathPool;

		public WorldReachability reachability;

		public FactionManager factionManager;

		public UniqueIDsManager uniqueIDsManager;

		public WorldPawns worldPawns;

		private static List<int> tmpNeighbors = new List<int>();

		private static List<Rot4> tmpOceanDirs = new List<Rot4>();

		public float PlanetCoverage
		{
			get
			{
				return this.info.planetCoverage;
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<WorldInfo>(ref this.info, "info", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				WorldGenerator_Grid.GenerateGridIntoWorld(this.info.seedString);
				this.ConstructComponents();
			}
			this.ExposeComponents();
		}

		private void ExposeComponents()
		{
			Scribe_Deep.LookDeep<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
			Scribe_Deep.LookDeep<FactionManager>(ref this.factionManager, "factionManager", new object[0]);
			Scribe_Deep.LookDeep<WorldPawns>(ref this.worldPawns, "worldPawns", new object[0]);
			Scribe_Deep.LookDeep<WorldObjectsHolder>(ref this.worldObjects, "worldObjects", new object[0]);
		}

		public void ConstructComponents()
		{
			this.renderer = new WorldRenderer();
			this.worldObjects = new WorldObjectsHolder();
			this.UI = new WorldInterface();
			this.debugDrawer = new WorldDebugDrawer();
			this.dynamicDrawManager = new WorldDynamicDrawManager();
			this.pathFinder = new WorldPathFinder();
			this.pathPool = new WorldPathPool();
			this.reachability = new WorldReachability();
			this.factionManager = new FactionManager();
			this.uniqueIDsManager = new UniqueIDsManager();
			this.worldPawns = new WorldPawns();
		}

		public void FinalizeInit()
		{
			this.pathGrid.RecalculateAllPerceivedPathCosts();
			AmbientSoundManager.EnsureWorldAmbientSoundCreated();
		}

		public void WorldTick()
		{
			this.worldPawns.WorldPawnsTick();
			this.factionManager.FactionManagerTick();
			this.worldObjects.WorldObjectsHolderTick();
			this.debugDrawer.WorldDebugDrawerTick();
			this.pathGrid.WorldPathGridTick();
		}

		public void WorldUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			this.renderer.CheckActivateWorldCamera();
			if (worldRenderedNow)
			{
				ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
				this.renderer.DrawWorldLayers();
				this.dynamicDrawManager.DrawDynamicWorldObjects();
				NoiseDebugUI.RenderPlanetNoise();
			}
		}

		public Rot4 CoastDirectionAt(int tileID)
		{
			Tile tile = this.grid[tileID];
			if (!tile.biome.canBuildBase)
			{
				return Rot4.Invalid;
			}
			World.tmpOceanDirs.Clear();
			this.grid.GetTileNeighbors(tileID, World.tmpNeighbors);
			int i = 0;
			int count = World.tmpNeighbors.Count;
			while (i < count)
			{
				Tile tile2 = this.grid[World.tmpNeighbors[i]];
				if (tile2.biome == BiomeDefOf.Ocean)
				{
					Rot4 rotFromTo = this.grid.GetRotFromTo(tileID, World.tmpNeighbors[i]);
					if (!World.tmpOceanDirs.Contains(rotFromTo))
					{
						World.tmpOceanDirs.Add(rotFromTo);
					}
				}
				i++;
			}
			if (World.tmpOceanDirs.Count == 0)
			{
				return Rot4.Invalid;
			}
			Rand.PushSeed();
			Rand.Seed = tileID;
			int index = Rand.Range(0, World.tmpOceanDirs.Count);
			Rand.PopSeed();
			return World.tmpOceanDirs[index];
		}

		public IEnumerable<ThingDef> NaturalRockTypesIn(int tile)
		{
			Rand.PushSeed();
			Rand.Seed = tile;
			List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock
			select d).ToList<ThingDef>();
			int num = Rand.RangeInclusive(2, 3);
			if (num > list.Count)
			{
				num = list.Count;
			}
			List<ThingDef> list2 = new List<ThingDef>();
			for (int i = 0; i < num; i++)
			{
				ThingDef item = list.RandomElement<ThingDef>();
				list.Remove(item);
				list2.Add(item);
			}
			Rand.PopSeed();
			return list2;
		}

		public bool Impassable(int tileID)
		{
			return !this.pathGrid.Passable(tileID);
		}
	}
}
