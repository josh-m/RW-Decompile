using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld.Planet
{
	public class WorldObjectsHolder : IExposable
	{
		private List<WorldObject> worldObjects = new List<WorldObject>();

		private List<Caravan> caravans = new List<Caravan>();

		private List<FactionBase> factionBases = new List<FactionBase>();

		private List<TravelingTransportPods> travelingTransportPods = new List<TravelingTransportPods>();

		private List<Settlement> settlements = new List<Settlement>();

		private List<DestroyedFactionBase> destroyedFactionBases = new List<DestroyedFactionBase>();

		private List<RoutePlannerWaypoint> routePlannerWaypoints = new List<RoutePlannerWaypoint>();

		private List<MapParent> mapParents = new List<MapParent>();

		private List<Site> sites = new List<Site>();

		private List<PeaceTalks> peaceTalks = new List<PeaceTalks>();

		private static List<WorldObject> tmpUnsavedWorldObjects = new List<WorldObject>();

		private static List<WorldObject> tmpWorldObjects = new List<WorldObject>();

		public List<WorldObject> AllWorldObjects
		{
			get
			{
				return this.worldObjects;
			}
		}

		public List<Caravan> Caravans
		{
			get
			{
				return this.caravans;
			}
		}

		public List<FactionBase> FactionBases
		{
			get
			{
				return this.factionBases;
			}
		}

		public List<TravelingTransportPods> TravelingTransportPods
		{
			get
			{
				return this.travelingTransportPods;
			}
		}

		public List<Settlement> Settlements
		{
			get
			{
				return this.settlements;
			}
		}

		public List<DestroyedFactionBase> DestroyedFactionBases
		{
			get
			{
				return this.destroyedFactionBases;
			}
		}

		public List<RoutePlannerWaypoint> RoutePlannerWaypoints
		{
			get
			{
				return this.routePlannerWaypoints;
			}
		}

		public List<MapParent> MapParents
		{
			get
			{
				return this.mapParents;
			}
		}

		public List<Site> Sites
		{
			get
			{
				return this.sites;
			}
		}

		public List<PeaceTalks> PeaceTalks
		{
			get
			{
				return this.peaceTalks;
			}
		}

		public int WorldObjectsCount
		{
			get
			{
				return this.worldObjects.Count;
			}
		}

		public int CaravansCount
		{
			get
			{
				return this.caravans.Count;
			}
		}

		public int RoutePlannerWaypointsCount
		{
			get
			{
				return this.routePlannerWaypoints.Count;
			}
		}

		public int PlayerControlledCaravansCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.caravans.Count; i++)
				{
					if (this.caravans[i].IsPlayerControlled)
					{
						num++;
					}
				}
				return num;
			}
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				WorldObjectsHolder.tmpUnsavedWorldObjects.Clear();
				for (int i = this.worldObjects.Count - 1; i >= 0; i--)
				{
					if (!this.worldObjects[i].def.saved)
					{
						WorldObjectsHolder.tmpUnsavedWorldObjects.Add(this.worldObjects[i]);
						this.worldObjects.RemoveAt(i);
					}
				}
			}
			Scribe_Collections.Look<WorldObject>(ref this.worldObjects, "worldObjects", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.worldObjects.AddRange(WorldObjectsHolder.tmpUnsavedWorldObjects);
				WorldObjectsHolder.tmpUnsavedWorldObjects.Clear();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.worldObjects.RemoveAll((WorldObject wo) => wo == null);
				for (int j = 0; j < this.worldObjects.Count; j++)
				{
					this.worldObjects[j].SpawnSetup();
				}
				this.Recache();
			}
		}

		public void Add(WorldObject o)
		{
			if (this.worldObjects.Contains(o))
			{
				Log.Error("Tried to add world object " + o + " to world, but it's already here.");
				return;
			}
			if (o.Tile < 0)
			{
				Log.Error("Tried to add world object " + o + " but its tile is not set. Setting to 0.");
				o.Tile = 0;
			}
			this.worldObjects.Add(o);
			this.AddToCache(o);
			o.SpawnSetup();
			o.PostAdd();
		}

		public void Remove(WorldObject o)
		{
			if (!this.worldObjects.Contains(o))
			{
				Log.Error("Tried to remove world object " + o + " from world, but it's not here.");
				return;
			}
			this.worldObjects.Remove(o);
			this.RemoveFromCache(o);
			o.PostRemove();
		}

		public void WorldObjectsHolderTick()
		{
			WorldObjectsHolder.tmpWorldObjects.Clear();
			WorldObjectsHolder.tmpWorldObjects.AddRange(this.worldObjects);
			for (int i = 0; i < WorldObjectsHolder.tmpWorldObjects.Count; i++)
			{
				WorldObjectsHolder.tmpWorldObjects[i].Tick();
			}
		}

		private void AddToCache(WorldObject o)
		{
			if (o is Caravan)
			{
				this.caravans.Add((Caravan)o);
			}
			if (o is FactionBase)
			{
				this.factionBases.Add((FactionBase)o);
			}
			if (o is TravelingTransportPods)
			{
				this.travelingTransportPods.Add((TravelingTransportPods)o);
			}
			if (o is Settlement)
			{
				this.settlements.Add((Settlement)o);
			}
			if (o is DestroyedFactionBase)
			{
				this.destroyedFactionBases.Add((DestroyedFactionBase)o);
			}
			if (o is RoutePlannerWaypoint)
			{
				this.routePlannerWaypoints.Add((RoutePlannerWaypoint)o);
			}
			if (o is MapParent)
			{
				this.mapParents.Add((MapParent)o);
			}
			if (o is Site)
			{
				this.sites.Add((Site)o);
			}
			if (o is PeaceTalks)
			{
				this.peaceTalks.Add((PeaceTalks)o);
			}
		}

		private void RemoveFromCache(WorldObject o)
		{
			if (o is Caravan)
			{
				this.caravans.Remove((Caravan)o);
			}
			if (o is FactionBase)
			{
				this.factionBases.Remove((FactionBase)o);
			}
			if (o is TravelingTransportPods)
			{
				this.travelingTransportPods.Remove((TravelingTransportPods)o);
			}
			if (o is Settlement)
			{
				this.settlements.Remove((Settlement)o);
			}
			if (o is DestroyedFactionBase)
			{
				this.destroyedFactionBases.Remove((DestroyedFactionBase)o);
			}
			if (o is RoutePlannerWaypoint)
			{
				this.routePlannerWaypoints.Remove((RoutePlannerWaypoint)o);
			}
			if (o is MapParent)
			{
				this.mapParents.Remove((MapParent)o);
			}
			if (o is Site)
			{
				this.sites.Remove((Site)o);
			}
			if (o is PeaceTalks)
			{
				this.peaceTalks.Remove((PeaceTalks)o);
			}
		}

		private void Recache()
		{
			this.caravans.Clear();
			this.factionBases.Clear();
			this.travelingTransportPods.Clear();
			this.settlements.Clear();
			this.destroyedFactionBases.Clear();
			this.routePlannerWaypoints.Clear();
			this.mapParents.Clear();
			this.sites.Clear();
			this.peaceTalks.Clear();
			for (int i = 0; i < this.worldObjects.Count; i++)
			{
				this.AddToCache(this.worldObjects[i]);
			}
		}

		public bool Contains(WorldObject o)
		{
			if (o == null)
			{
				return false;
			}
			if (o is Caravan)
			{
				return this.caravans.Contains((Caravan)o);
			}
			if (o is Settlement)
			{
				return this.settlements.Contains((Settlement)o);
			}
			return this.worldObjects.Contains(o);
		}

		[DebuggerHidden]
		public IEnumerable<WorldObject> ObjectsAt(int tileID)
		{
			if (tileID >= 0)
			{
				for (int i = 0; i < this.worldObjects.Count; i++)
				{
					if (this.worldObjects[i].Tile == tileID)
					{
						yield return this.worldObjects[i];
					}
				}
			}
		}

		public bool AnyWorldObjectAt(int tile)
		{
			for (int i = 0; i < this.worldObjects.Count; i++)
			{
				if (this.worldObjects[i].Tile == tile)
				{
					return true;
				}
			}
			return false;
		}

		public bool AnyWorldObjectAt<T>(int tile) where T : WorldObject
		{
			return this.WorldObjectAt<T>(tile) != null;
		}

		public T WorldObjectAt<T>(int tile) where T : WorldObject
		{
			for (int i = 0; i < this.worldObjects.Count; i++)
			{
				if (this.worldObjects[i].Tile == tile && this.worldObjects[i] is T)
				{
					return this.worldObjects[i] as T;
				}
			}
			return (T)((object)null);
		}

		public bool AnyWorldObjectAt(int tile, WorldObjectDef def)
		{
			return this.WorldObjectAt(tile, def) != null;
		}

		public WorldObject WorldObjectAt(int tile, WorldObjectDef def)
		{
			for (int i = 0; i < this.worldObjects.Count; i++)
			{
				if (this.worldObjects[i].Tile == tile && this.worldObjects[i].def == def)
				{
					return this.worldObjects[i];
				}
			}
			return null;
		}

		public bool AnyFactionBaseAt(int tile)
		{
			return this.FactionBaseAt(tile) != null;
		}

		public FactionBase FactionBaseAt(int tile)
		{
			for (int i = 0; i < this.factionBases.Count; i++)
			{
				if (this.factionBases[i].Tile == tile)
				{
					return this.factionBases[i];
				}
			}
			return null;
		}

		public bool AnySettlementAt(int tile)
		{
			return this.SettlementAt(tile) != null;
		}

		public Settlement SettlementAt(int tile)
		{
			for (int i = 0; i < this.settlements.Count; i++)
			{
				if (this.settlements[i].Tile == tile)
				{
					return this.settlements[i];
				}
			}
			return null;
		}

		public bool AnyDestroyedFactionBaseAt(int tile)
		{
			return this.DestroyedFactionBaseAt(tile) != null;
		}

		public DestroyedFactionBase DestroyedFactionBaseAt(int tile)
		{
			for (int i = 0; i < this.destroyedFactionBases.Count; i++)
			{
				if (this.destroyedFactionBases[i].Tile == tile)
				{
					return this.destroyedFactionBases[i];
				}
			}
			return null;
		}

		public bool AnyMapParentAt(int tile)
		{
			return this.MapParentAt(tile) != null;
		}

		public MapParent MapParentAt(int tile)
		{
			for (int i = 0; i < this.mapParents.Count; i++)
			{
				if (this.mapParents[i].Tile == tile)
				{
					return this.mapParents[i];
				}
			}
			return null;
		}

		public bool AnyWorldObjectOfDefAt(WorldObjectDef def, int tile)
		{
			return this.WorldObjectOfDefAt(def, tile) != null;
		}

		public WorldObject WorldObjectOfDefAt(WorldObjectDef def, int tile)
		{
			for (int i = 0; i < this.worldObjects.Count; i++)
			{
				if (this.worldObjects[i].def == def && this.worldObjects[i].Tile == tile)
				{
					return this.worldObjects[i];
				}
			}
			return null;
		}

		public Caravan PlayerControlledCaravanAt(int tile)
		{
			for (int i = 0; i < this.caravans.Count; i++)
			{
				if (this.caravans[i].Tile == tile && this.caravans[i].IsPlayerControlled)
				{
					return this.caravans[i];
				}
			}
			return null;
		}

		public bool AnySettlementAtOrAdjacent(int tile)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			for (int i = 0; i < this.settlements.Count; i++)
			{
				if (worldGrid.IsNeighborOrSame(this.settlements[i].Tile, tile))
				{
					return true;
				}
			}
			return false;
		}

		public RoutePlannerWaypoint RoutePlannerWaypointAt(int tile)
		{
			for (int i = 0; i < this.routePlannerWaypoints.Count; i++)
			{
				if (this.routePlannerWaypoints[i].Tile == tile)
				{
					return this.routePlannerWaypoints[i];
				}
			}
			return null;
		}

		public void GetPlayerControlledCaravansAt(int tile, List<Caravan> outCaravans)
		{
			outCaravans.Clear();
			for (int i = 0; i < this.caravans.Count; i++)
			{
				Caravan caravan = this.caravans[i];
				if (caravan.Tile == tile && caravan.IsPlayerControlled)
				{
					outCaravans.Add(caravan);
				}
			}
		}
	}
}
