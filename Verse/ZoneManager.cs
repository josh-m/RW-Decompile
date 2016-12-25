using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class ZoneManager : IExposable
	{
		public Map map;

		private List<Zone> allZones = new List<Zone>();

		private Zone[] zoneGrid;

		public List<Zone> AllZones
		{
			get
			{
				return this.allZones;
			}
		}

		public ZoneManager(Map map)
		{
			this.map = map;
			this.zoneGrid = new Zone[map.cellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Zone>(ref this.allZones, "allZones", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.UpdateZoneManagerLinks();
				this.RebuildZoneGrid();
			}
		}

		private void UpdateZoneManagerLinks()
		{
			for (int i = 0; i < this.allZones.Count; i++)
			{
				this.allZones[i].zoneManager = this;
			}
		}

		private void RebuildZoneGrid()
		{
			CellIndices cellIndices = this.map.cellIndices;
			this.zoneGrid = new Zone[cellIndices.NumGridCells];
			foreach (Zone current in this.allZones)
			{
				foreach (IntVec3 current2 in current)
				{
					this.zoneGrid[cellIndices.CellToIndex(current2)] = current;
				}
			}
		}

		public void RegisterZone(Zone newZone)
		{
			this.allZones.Add(newZone);
		}

		public void DeregisterZone(Zone oldZone)
		{
			this.allZones.Remove(oldZone);
		}

		internal void AddZoneGridCell(Zone zone, IntVec3 c)
		{
			this.zoneGrid[this.map.cellIndices.CellToIndex(c)] = zone;
		}

		internal void ClearZoneGridCell(IntVec3 c)
		{
			this.zoneGrid[this.map.cellIndices.CellToIndex(c)] = null;
		}

		public Zone ZoneAt(IntVec3 c)
		{
			return this.zoneGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public string NewZoneName(string nameBase)
		{
			for (int i = 1; i <= 1000; i++)
			{
				string cand = nameBase + " " + i;
				if (!this.allZones.Any((Zone z) => z.label == cand))
				{
					return cand;
				}
			}
			Log.Error("Ran out of zone names.");
			return "Zone X";
		}

		internal void Notify_NoZoneOverlapThingSpawned(Thing thing)
		{
			CellRect cellRect = thing.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					Zone zone = this.ZoneAt(c);
					if (zone != null)
					{
						zone.RemoveCell(c);
						zone.CheckContiguous();
					}
				}
			}
		}
	}
}
