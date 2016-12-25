using System;
using Verse;

namespace RimWorld
{
	public class StrengthWatcher
	{
		private Map map;

		public float StrengthRating
		{
			get
			{
				float num = 0f;
				foreach (Pawn current in this.map.mapPawns.FreeColonists)
				{
					float num2 = 1f;
					num2 *= current.health.summaryHealth.SummaryHealthPercent;
					if (current.Downed)
					{
						num2 *= 0.3f;
					}
					num += num2;
				}
				foreach (Building current2 in this.map.listerBuildings.allBuildingsColonistCombatTargets)
				{
					if (current2.def.building != null && current2.def.building.IsTurret)
					{
						num += 0.3f;
					}
				}
				return num;
			}
		}

		public StrengthWatcher(Map map)
		{
			this.map = map;
		}
	}
}
