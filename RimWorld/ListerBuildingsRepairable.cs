using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class ListerBuildingsRepairable
	{
		private static Dictionary<Faction, List<Thing>> repairables = new Dictionary<Faction, List<Thing>>();

		public static void Reinit()
		{
			ListerBuildingsRepairable.repairables.Clear();
		}

		public static List<Thing> RepairableBuildings(Faction fac)
		{
			return ListerBuildingsRepairable.ListFor(fac);
		}

		public static void Notify_BuildingSpawned(Building b)
		{
			if (b.Faction == null)
			{
				return;
			}
			ListerBuildingsRepairable.UpdateBuilding(b);
		}

		public static void Notify_BuildingDeSpawned(Building b)
		{
			if (b.Faction == null)
			{
				return;
			}
			List<Thing> list = ListerBuildingsRepairable.ListFor(b.Faction);
			if (list.Contains(b))
			{
				list.Remove(b);
			}
		}

		public static void Notify_BuildingTookDamage(Building b)
		{
			if (b.Faction == null)
			{
				return;
			}
			ListerBuildingsRepairable.UpdateBuilding(b);
		}

		internal static void Notify_BuildingFactionChanged(Building b)
		{
			ListerBuildingsRepairable.Notify_BuildingDeSpawned(b);
			ListerBuildingsRepairable.Notify_BuildingSpawned(b);
		}

		private static void UpdateBuilding(Building b)
		{
			if (b.Faction == null || !b.def.building.repairable)
			{
				return;
			}
			List<Thing> list = ListerBuildingsRepairable.ListFor(b.Faction);
			if (b.HitPoints < b.MaxHitPoints)
			{
				if (!list.Contains(b))
				{
					list.Add(b);
				}
			}
			else if (list.Contains(b))
			{
				list.Remove(b);
			}
		}

		private static List<Thing> ListFor(Faction fac)
		{
			List<Thing> list;
			if (!ListerBuildingsRepairable.repairables.TryGetValue(fac, out list))
			{
				list = new List<Thing>();
				ListerBuildingsRepairable.repairables.Add(fac, list);
			}
			return list;
		}

		internal static string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				List<Thing> list = ListerBuildingsRepairable.ListFor(current);
				if (!list.NullOrEmpty<Thing>())
				{
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"=======",
						current.Name,
						" (",
						current.def,
						")"
					}));
					foreach (Thing current2 in list)
					{
						stringBuilder.AppendLine(current2.ThingID);
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
