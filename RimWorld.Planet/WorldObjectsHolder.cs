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
			Scribe_Collections.LookList<WorldObject>(ref this.worldObjects, "worldObjects", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < this.worldObjects.Count; i++)
				{
					this.worldObjects[i].SpawnSetup();
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
			Caravan caravan = o as Caravan;
			if (caravan != null)
			{
				this.caravans.Add(caravan);
			}
			FactionBase factionBase = o as FactionBase;
			if (factionBase != null)
			{
				this.factionBases.Add(factionBase);
			}
			TravelingTransportPods travelingTransportPods = o as TravelingTransportPods;
			if (travelingTransportPods != null)
			{
				this.travelingTransportPods.Add(travelingTransportPods);
			}
		}

		private void RemoveFromCache(WorldObject o)
		{
			Caravan caravan = o as Caravan;
			if (caravan != null)
			{
				this.caravans.Remove(caravan);
			}
			FactionBase factionBase = o as FactionBase;
			if (factionBase != null)
			{
				this.factionBases.Remove(factionBase);
			}
			TravelingTransportPods travelingTransportPods = o as TravelingTransportPods;
			if (travelingTransportPods != null)
			{
				this.travelingTransportPods.Remove(travelingTransportPods);
			}
		}

		private void Recache()
		{
			this.caravans.Clear();
			this.factionBases.Clear();
			this.travelingTransportPods.Clear();
			for (int i = 0; i < this.worldObjects.Count; i++)
			{
				this.AddToCache(this.worldObjects[i]);
			}
		}

		public bool Contains(WorldObject o)
		{
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

		public bool AnyFactionBaseAt(int tile)
		{
			for (int i = 0; i < this.factionBases.Count; i++)
			{
				if (this.factionBases[i].Tile == tile)
				{
					return true;
				}
			}
			return false;
		}
	}
}
