using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedColonistBeds : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
				if (GenDate.DaysPassed > 30)
				{
					return false;
				}
				int num = 0;
				int num2 = 0;
				List<Building> allBuildingsColonist = Find.ListerBuildings.allBuildingsColonist;
				for (int i = 0; i < allBuildingsColonist.Count; i++)
				{
					Building_Bed building_Bed = allBuildingsColonist[i] as Building_Bed;
					if (building_Bed != null && !building_Bed.ForPrisoners && !building_Bed.Medical)
					{
						if (building_Bed.SleepingSlotsCount == 1)
						{
							num++;
						}
						else
						{
							num2++;
						}
					}
				}
				int num3 = 0;
				int num4 = 0;
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(current, false);
					if (pawn == null || !pawn.Spawned || pawn.Faction != Faction.OfPlayer || pawn.HostFaction != null)
					{
						num3++;
					}
					else
					{
						num4++;
					}
				}
				if (num4 % 2 != 0)
				{
					Log.ErrorOnce("partneredCols % 2 != 0", 743211);
				}
				for (int j = 0; j < num4 / 2; j++)
				{
					if (num2 > 0)
					{
						num2--;
					}
					else
					{
						num -= 2;
					}
				}
				for (int k = 0; k < num3; k++)
				{
					if (num2 > 0)
					{
						num2--;
					}
					else
					{
						num--;
					}
				}
				return num < 0 || num2 < 0;
			}
		}

		public Alert_NeedColonistBeds()
		{
			this.baseLabel = "NeedColonistBeds".Translate();
			this.baseExplanation = "NeedColonistBedsDesc".Translate();
		}
	}
}
