using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public sealed class ResourceCounter
	{
		private Map map;

		private Dictionary<ThingDef, int> countedAmounts = new Dictionary<ThingDef, int>();

		public int Silver
		{
			get
			{
				return this.GetCount(ThingDefOf.Silver);
			}
		}

		public int Metal
		{
			get
			{
				return this.GetCount(ThingDefOf.Steel);
			}
		}

		public float TotalHumanEdibleNutrition
		{
			get
			{
				float num = 0f;
				foreach (KeyValuePair<ThingDef, int> current in this.countedAmounts)
				{
					if (current.Key.IsNutritionGivingIngestible && current.Key.ingestible.HumanEdible)
					{
						num += current.Key.ingestible.nutrition * (float)current.Value;
					}
				}
				return num;
			}
		}

		public Dictionary<ThingDef, int> AllCountedAmounts
		{
			get
			{
				return this.countedAmounts;
			}
		}

		public ResourceCounter(Map map)
		{
			this.map = map;
			this.ResetResourceCounts();
		}

		public void ResetResourceCounts()
		{
			this.countedAmounts.Clear();
			foreach (ThingDef current in from def in DefDatabase<ThingDef>.AllDefs
			where def.CountAsResource
			orderby def.resourceReadoutPriority descending
			select def)
			{
				this.countedAmounts.Add(current, 0);
			}
		}

		public int GetCount(ThingDef rDef)
		{
			if (rDef.resourceReadoutPriority == ResourceCountPriority.Uncounted)
			{
				return 0;
			}
			int result;
			if (this.countedAmounts.TryGetValue(rDef, out result))
			{
				return result;
			}
			Log.Error("Looked for nonexistent key " + rDef + " in counted resources.");
			this.countedAmounts.Add(rDef, 0);
			return 0;
		}

		public int GetCountIn(ThingRequestGroup group)
		{
			int num = 0;
			foreach (KeyValuePair<ThingDef, int> current in this.countedAmounts)
			{
				if (group.Includes(current.Key))
				{
					num += current.Value;
				}
			}
			return num;
		}

		public int GetCountIn(ThingCategoryDef cat)
		{
			int num = 0;
			for (int i = 0; i < cat.childThingDefs.Count; i++)
			{
				num += this.GetCount(cat.childThingDefs[i]);
			}
			for (int j = 0; j < cat.childCategories.Count; j++)
			{
				if (!cat.childCategories[j].resourceReadoutRoot)
				{
					num += this.GetCountIn(cat.childCategories[j]);
				}
			}
			return num;
		}

		public void ResourceCounterTick()
		{
			if (Find.TickManager.TicksGame % 204 == 0)
			{
				this.UpdateResourceCounts();
			}
		}

		public void UpdateResourceCounts()
		{
			this.ResetResourceCounts();
			List<SlotGroup> allGroupsListForReading = this.map.slotGroupManager.AllGroupsListForReading;
			for (int i = 0; i < allGroupsListForReading.Count; i++)
			{
				SlotGroup slotGroup = allGroupsListForReading[i];
				foreach (Thing current in slotGroup.HeldThings)
				{
					if (current.def.CountAsResource && this.ShouldCount(current))
					{
						Dictionary<ThingDef, int> dictionary;
						Dictionary<ThingDef, int> expr_62 = dictionary = this.countedAmounts;
						ThingDef def;
						ThingDef expr_6B = def = current.def;
						int num = dictionary[def];
						expr_62[expr_6B] = num + current.stackCount;
					}
				}
			}
		}

		private bool ShouldCount(Thing t)
		{
			return !t.IsNotFresh();
		}
	}
}
