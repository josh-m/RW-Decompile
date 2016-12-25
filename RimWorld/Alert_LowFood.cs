using System;
using Verse;

namespace RimWorld
{
	public class Alert_LowFood : Alert_High
	{
		private const float NutritionThresholdPerColonist = 4f;

		public override string FullExplanation
		{
			get
			{
				return string.Format("LowFoodDesc".Translate(), Find.ResourceCounter.TotalHumanEdibleNutrition.ToString("F0"));
			}
		}

		public override AlertReport Report
		{
			get
			{
				if (Find.TickManager.TicksGame < 150000)
				{
					return false;
				}
				return Find.ResourceCounter.TotalHumanEdibleNutrition < 4f * (float)Find.MapPawns.FreeColonistsSpawnedCount;
			}
		}

		public Alert_LowFood()
		{
			this.baseLabel = "LowFood".Translate();
		}
	}
}
