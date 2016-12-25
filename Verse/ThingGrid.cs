using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public sealed class ThingGrid
	{
		private Map map;

		private List<Thing>[] thingGrid;

		private static readonly List<Thing> EmptyThingList = new List<Thing>();

		public ThingGrid(Map map)
		{
			this.map = map;
			CellIndices cellIndices = map.cellIndices;
			this.thingGrid = new List<Thing>[cellIndices.NumGridCells];
			for (int i = 0; i < cellIndices.NumGridCells; i++)
			{
				this.thingGrid[i] = new List<Thing>(16);
			}
		}

		public void Register(Thing t)
		{
			if (t.def.size.x == 1 && t.def.size.z == 1)
			{
				this.RegisterInCell(t, t.Position);
			}
			else
			{
				CellRect cellRect = t.OccupiedRect();
				for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
				{
					for (int j = cellRect.minX; j <= cellRect.maxX; j++)
					{
						this.RegisterInCell(t, new IntVec3(j, 0, i));
					}
				}
			}
		}

		private void RegisterInCell(Thing t, IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				Log.Warning(string.Concat(new object[]
				{
					t,
					" tried to register out of bounds at ",
					c,
					". Destroying."
				}));
				t.Destroy(DestroyMode.Vanish);
				return;
			}
			this.thingGrid[this.map.cellIndices.CellToIndex(c)].Add(t);
		}

		public void Deregister(Thing t, bool doEvenIfDespawned = false)
		{
			if (!t.Spawned && !doEvenIfDespawned)
			{
				return;
			}
			if (t.def.size.x == 1 && t.def.size.z == 1)
			{
				this.DeregisterInCell(t, t.Position);
			}
			else
			{
				CellRect cellRect = t.OccupiedRect();
				for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
				{
					for (int j = cellRect.minX; j <= cellRect.maxX; j++)
					{
						this.DeregisterInCell(t, new IntVec3(j, 0, i));
					}
				}
			}
		}

		private void DeregisterInCell(Thing t, IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				Log.Error(t + " tried to de-register out of bounds at " + c);
				return;
			}
			int num = this.map.cellIndices.CellToIndex(c);
			if (this.thingGrid[num].Contains(t))
			{
				this.thingGrid[num].Remove(t);
			}
		}

		[DebuggerHidden]
		public IEnumerable<Thing> ThingsAt(IntVec3 c)
		{
			if (c.InBounds(this.map))
			{
				List<Thing> list = this.thingGrid[this.map.cellIndices.CellToIndex(c)];
				for (int i = 0; i < list.Count; i++)
				{
					yield return list[i];
				}
			}
		}

		public List<Thing> ThingsListAt(IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				Log.ErrorOnce("Got ThingsListAt out of bounds: " + c, 495287);
				return ThingGrid.EmptyThingList;
			}
			return this.thingGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public List<Thing> ThingsListAtFast(IntVec3 c)
		{
			return this.thingGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public List<Thing> ThingsListAtFast(int index)
		{
			return this.thingGrid[index];
		}

		public bool CellContains(IntVec3 c, ThingCategory cat)
		{
			return this.ThingAt(c, cat) != null;
		}

		public Thing ThingAt(IntVec3 c, ThingCategory cat)
		{
			if (!c.InBounds(this.map))
			{
				return null;
			}
			List<Thing> list = this.thingGrid[this.map.cellIndices.CellToIndex(c)];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.category == cat)
				{
					return list[i];
				}
			}
			return null;
		}

		public bool CellContains(IntVec3 c, ThingDef def)
		{
			return this.ThingAt(c, def) != null;
		}

		public Thing ThingAt(IntVec3 c, ThingDef def)
		{
			if (!c.InBounds(this.map))
			{
				return null;
			}
			List<Thing> list = this.thingGrid[this.map.cellIndices.CellToIndex(c)];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def == def)
				{
					return list[i];
				}
			}
			return null;
		}

		public T ThingAt<T>(IntVec3 c) where T : Thing
		{
			if (!c.InBounds(this.map))
			{
				return (T)((object)null);
			}
			List<Thing> list = this.thingGrid[this.map.cellIndices.CellToIndex(c)];
			for (int i = 0; i < list.Count; i++)
			{
				T t = list[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}
	}
}
