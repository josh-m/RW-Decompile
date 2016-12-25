using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_LowFood : Alert
	{
		private const float NutritionThresholdPerColonist = 4f;

		public Alert_LowFood()
		{
			this.defaultLabel = "LowFood".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			Map map = this.MapWithLowFood();
			if (map == null)
			{
				return string.Empty;
			}
			float totalHumanEdibleNutrition = map.resourceCounter.TotalHumanEdibleNutrition;
			int num = map.mapPawns.FreeColonistsSpawnedCount + (from pr in map.mapPawns.PrisonersOfColony
			where pr.guest.GetsFood
			select pr).Count<Pawn>();
			int num2 = Mathf.FloorToInt(totalHumanEdibleNutrition / (float)num);
			return string.Format("LowFoodDesc".Translate(), totalHumanEdibleNutrition.ToString("F0"), num.ToStringCached(), num2.ToStringCached());
		}

		public override AlertReport GetReport()
		{
			if (Find.TickManager.TicksGame < 150000)
			{
				return false;
			}
			return this.MapWithLowFood() != null;
		}

		private Map MapWithLowFood()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
				{
					int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
					if (map.resourceCounter.TotalHumanEdibleNutrition < 4f * (float)freeColonistsSpawnedCount)
					{
						return map;
					}
				}
			}
			return null;
		}
	}
}
