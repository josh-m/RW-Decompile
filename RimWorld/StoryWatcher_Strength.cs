using System;
using Verse;

namespace RimWorld
{
	public class StoryWatcher_Strength
	{
		public float StrengthRating
		{
			get
			{
				float num = 0f;
				foreach (Pawn current in Find.MapPawns.FreeColonists)
				{
					if (!current.ThreatDisabled())
					{
						num += 1f;
					}
					else
					{
						num += 0.4f;
					}
				}
				foreach (Building current2 in Find.ListerBuildings.allBuildingsColonistCombatTargets)
				{
					if (current2.def.building != null && current2.def.building.IsTurret)
					{
						num += 0.4f;
					}
				}
				return num;
			}
		}
	}
}
