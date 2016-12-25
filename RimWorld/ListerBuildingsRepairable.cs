using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ListerBuildingsRepairable
	{
		private Dictionary<Faction, List<Thing>> repairables = new Dictionary<Faction, List<Thing>>();

		public List<Thing> RepairableBuildings(Faction fac)
		{
			return this.ListFor(fac);
		}

		public void Notify_BuildingSpawned(Building b)
		{
			if (b.Faction == null)
			{
				return;
			}
			this.UpdateBuilding(b);
		}

		public void Notify_BuildingDeSpawned(Building b)
		{
			if (b.Faction == null)
			{
				return;
			}
			List<Thing> list = this.ListFor(b.Faction);
			if (list.Contains(b))
			{
				list.Remove(b);
			}
		}

		public void Notify_BuildingTookDamage(Building b)
		{
			if (b.Faction == null)
			{
				return;
			}
			this.UpdateBuilding(b);
		}

		internal void Notify_BuildingFactionChanged(Building b)
		{
			this.Notify_BuildingDeSpawned(b);
			this.Notify_BuildingSpawned(b);
		}

		private void UpdateBuilding(Building b)
		{
			if (b.Faction == null || !b.def.building.repairable)
			{
				return;
			}
			List<Thing> list = this.ListFor(b.Faction);
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

		private List<Thing> ListFor(Faction fac)
		{
			List<Thing> list;
			if (!this.repairables.TryGetValue(fac, out list))
			{
				list = new List<Thing>();
				this.repairables.Add(fac, list);
			}
			return list;
		}

		internal string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				List<Thing> list = this.ListFor(current);
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
