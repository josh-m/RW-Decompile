using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ListerHaulables
	{
		private const int CellsPerTick = 4;

		private static List<Thing> haulables = new List<Thing>();

		private static int groupCycleIndex = 0;

		private static List<int> cellCycleIndices = new List<int>();

		private static string debugOutput = "uninitialized";

		public static void Reinit()
		{
			ListerHaulables.haulables.Clear();
		}

		public static List<Thing> ThingsPotentiallyNeedingHauling()
		{
			return ListerHaulables.haulables;
		}

		public static void Notify_Spawned(Thing t)
		{
			ListerHaulables.CheckAdd(t);
		}

		public static void Notify_DeSpawned(Thing t)
		{
			ListerHaulables.TryRemove(t);
		}

		public static void HaulDesignationAdded(Thing t)
		{
			ListerHaulables.CheckAdd(t);
		}

		public static void HaulDesignationRemoved(Thing t)
		{
			ListerHaulables.TryRemove(t);
		}

		public static void Notify_Unforbidden(Thing t)
		{
			ListerHaulables.CheckAdd(t);
		}

		public static void Notify_Forbidden(Thing t)
		{
			ListerHaulables.TryRemove(t);
		}

		public static void Notify_SlotGroupChanged(SlotGroup sg)
		{
			if (sg.CellsList != null)
			{
				for (int i = 0; i < sg.CellsList.Count; i++)
				{
					ListerHaulables.RecalcAllInCell(sg.CellsList[i]);
				}
			}
		}

		public static void ListerHaulablesTick()
		{
			ListerHaulables.groupCycleIndex++;
			if (ListerHaulables.groupCycleIndex >= 2147473647)
			{
				ListerHaulables.groupCycleIndex = 0;
			}
			List<SlotGroup> allGroupsListForReading = Find.SlotGroupManager.AllGroupsListForReading;
			if (allGroupsListForReading.Count == 0)
			{
				return;
			}
			int num = ListerHaulables.groupCycleIndex % allGroupsListForReading.Count;
			SlotGroup slotGroup = allGroupsListForReading[ListerHaulables.groupCycleIndex % allGroupsListForReading.Count];
			while (ListerHaulables.cellCycleIndices.Count <= num)
			{
				ListerHaulables.cellCycleIndices.Add(0);
			}
			if (ListerHaulables.cellCycleIndices[num] >= 2147473647)
			{
				ListerHaulables.cellCycleIndices[num] = 0;
			}
			for (int i = 0; i < 4; i++)
			{
				List<int> list;
				List<int> expr_A5 = list = ListerHaulables.cellCycleIndices;
				int num2;
				int expr_A9 = num2 = num;
				num2 = list[num2];
				expr_A5[expr_A9] = num2 + 1;
				IntVec3 c = slotGroup.CellsList[ListerHaulables.cellCycleIndices[num] % slotGroup.CellsList.Count];
				List<Thing> thingList = c.GetThingList();
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].def.EverHaulable)
					{
						ListerHaulables.Check(thingList[j]);
						break;
					}
				}
			}
		}

		public static void RecalcAllInCell(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList();
			for (int i = 0; i < thingList.Count; i++)
			{
				ListerHaulables.Check(thingList[i]);
			}
		}

		private static void Check(Thing t)
		{
			if (ListerHaulables.ShouldBeHaulable(t))
			{
				if (!ListerHaulables.haulables.Contains(t))
				{
					ListerHaulables.haulables.Add(t);
				}
			}
			else if (ListerHaulables.haulables.Contains(t))
			{
				ListerHaulables.haulables.Remove(t);
			}
		}

		private static bool ShouldBeHaulable(Thing t)
		{
			if (t.IsForbidden(Faction.OfPlayer))
			{
				return false;
			}
			if (!t.def.alwaysHaulable)
			{
				if (!t.def.EverHaulable)
				{
					return false;
				}
				if (Find.DesignationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInAnyStorage())
				{
					return false;
				}
			}
			return !t.IsInValidBestStorage();
		}

		private static void CheckAdd(Thing t)
		{
			if (ListerHaulables.ShouldBeHaulable(t) && !ListerHaulables.haulables.Contains(t))
			{
				ListerHaulables.haulables.Add(t);
			}
		}

		private static void TryRemove(Thing t)
		{
			if (t.def.category == ThingCategory.Item && ListerHaulables.haulables.Contains(t))
			{
				ListerHaulables.haulables.Remove(t);
			}
		}

		internal static string DebugString()
		{
			if (Time.frameCount % 10 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("======= All haulables (Count " + ListerHaulables.haulables.Count + ")");
				int num = 0;
				foreach (Thing current in ListerHaulables.haulables)
				{
					stringBuilder.AppendLine(current.ThingID);
					num++;
					if (num > 200)
					{
						break;
					}
				}
				ListerHaulables.debugOutput = stringBuilder.ToString();
			}
			return ListerHaulables.debugOutput;
		}
	}
}
