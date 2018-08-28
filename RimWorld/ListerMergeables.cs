using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ListerMergeables
	{
		private Map map;

		private List<Thing> mergeables = new List<Thing>();

		private string debugOutput = "uninitialized";

		public ListerMergeables(Map map)
		{
			this.map = map;
		}

		public List<Thing> ThingsPotentiallyNeedingMerging()
		{
			return this.mergeables;
		}

		public void Notify_Spawned(Thing t)
		{
			this.CheckAdd(t);
		}

		public void Notify_DeSpawned(Thing t)
		{
			this.TryRemove(t);
		}

		public void Notify_Unforbidden(Thing t)
		{
			this.CheckAdd(t);
		}

		public void Notify_Forbidden(Thing t)
		{
			this.TryRemove(t);
		}

		public void Notify_SlotGroupChanged(SlotGroup sg)
		{
			if (sg.CellsList != null)
			{
				for (int i = 0; i < sg.CellsList.Count; i++)
				{
					this.RecalcAllInCell(sg.CellsList[i]);
				}
			}
		}

		public void Notify_ThingStackChanged(Thing t)
		{
			this.Check(t);
		}

		public void RecalcAllInCell(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList(this.map);
			for (int i = 0; i < thingList.Count; i++)
			{
				this.Check(thingList[i]);
			}
		}

		private void Check(Thing t)
		{
			if (this.ShouldBeMergeable(t))
			{
				if (!this.mergeables.Contains(t))
				{
					this.mergeables.Add(t);
				}
			}
			else
			{
				this.mergeables.Remove(t);
			}
		}

		private bool ShouldBeMergeable(Thing t)
		{
			return !t.IsForbidden(Faction.OfPlayer) && t.GetSlotGroup() != null && t.stackCount != t.def.stackLimit;
		}

		private void CheckAdd(Thing t)
		{
			if (this.ShouldBeMergeable(t) && !this.mergeables.Contains(t))
			{
				this.mergeables.Add(t);
			}
		}

		private void TryRemove(Thing t)
		{
			if (t.def.category == ThingCategory.Item)
			{
				this.mergeables.Remove(t);
			}
		}

		internal string DebugString()
		{
			if (Time.frameCount % 10 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("======= All mergeables (Count " + this.mergeables.Count + ")");
				int num = 0;
				foreach (Thing current in this.mergeables)
				{
					stringBuilder.AppendLine(current.ThingID);
					num++;
					if (num > 200)
					{
						break;
					}
				}
				this.debugOutput = stringBuilder.ToString();
			}
			return this.debugOutput;
		}
	}
}
