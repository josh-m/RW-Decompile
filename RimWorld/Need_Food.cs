using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Food : Need
	{
		private const float BaseFoodFallPerTick = 2.66666666E-05f;

		private const float MalnutritionSeverityPerDay = 0.2f;

		private const float MalnutritionSeverityPerInterval = 0.00133333332f;

		private int lastNonStarvingTick = -99999;

		public bool Starving
		{
			get
			{
				return this.CurCategory == HungerCategory.Starving;
			}
		}

		public float PercentageThreshUrgentlyHungry
		{
			get
			{
				return this.pawn.RaceProps.FoodLevelPercentageWantEat * 0.4f;
			}
		}

		public float PercentageThreshHungry
		{
			get
			{
				return this.pawn.RaceProps.FoodLevelPercentageWantEat * 0.8f;
			}
		}

		public float NutritionBetweenHungryAndFed
		{
			get
			{
				return (1f - this.PercentageThreshHungry) * this.MaxLevel;
			}
		}

		public HungerCategory CurCategory
		{
			get
			{
				if (base.CurLevelPercentage <= 0f)
				{
					return HungerCategory.Starving;
				}
				if (base.CurLevelPercentage < this.PercentageThreshUrgentlyHungry)
				{
					return HungerCategory.UrgentlyHungry;
				}
				if (base.CurLevelPercentage < this.PercentageThreshHungry)
				{
					return HungerCategory.Hungry;
				}
				return HungerCategory.Fed;
			}
		}

		public float FoodFallPerTick
		{
			get
			{
				return this.FoodFallPerTickAssumingCategory(this.CurCategory);
			}
		}

		public int TicksUntilHungryWhenFed
		{
			get
			{
				return Mathf.CeilToInt(this.NutritionBetweenHungryAndFed / this.FoodFallPerTickAssumingCategory(HungerCategory.Fed));
			}
		}

		public override int GUIChangeArrow
		{
			get
			{
				return -1;
			}
		}

		public override float MaxLevel
		{
			get
			{
				return this.pawn.BodySize;
			}
		}

		public float NutritionWanted
		{
			get
			{
				return this.MaxLevel - this.CurLevel;
			}
		}

		private float HungerRate
		{
			get
			{
				float num = this.pawn.ageTracker.CurLifeStage.hungerRateFactor * this.pawn.RaceProps.baseHungerRate;
				List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					HediffStage curStage = hediffs[i].CurStage;
					if (curStage != null)
					{
						num *= curStage.hungerRateFactor;
					}
				}
				return num;
			}
		}

		public int TicksStarving
		{
			get
			{
				return Mathf.Max(0, Find.TickManager.TicksGame - this.lastNonStarvingTick);
			}
		}

		public Need_Food(Pawn pawn) : base(pawn)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.lastNonStarvingTick, "lastNonStarvingTick", -99999, false);
		}

		private float FoodFallPerTickAssumingCategory(HungerCategory cat)
		{
			switch (cat)
			{
			case HungerCategory.Fed:
				return 2.66666666E-05f * this.HungerRate;
			case HungerCategory.Hungry:
				return 2.66666666E-05f * this.HungerRate * 0.5f;
			case HungerCategory.UrgentlyHungry:
				return 2.66666666E-05f * this.HungerRate * 0.25f;
			case HungerCategory.Starving:
				return 2.66666666E-05f * this.HungerRate * 0.15f;
			default:
				return 999f;
			}
		}

		public override void NeedInterval()
		{
			this.CurLevel -= this.FoodFallPerTick * 150f;
			if (!this.Starving)
			{
				this.lastNonStarvingTick = Find.TickManager.TicksGame;
			}
			if (this.Starving)
			{
				HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.Malnutrition, 0.00133333332f);
			}
			else
			{
				HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.Malnutrition, -0.00133333332f);
			}
		}

		public override void SetInitialLevel()
		{
			if (this.pawn.RaceProps.Humanlike)
			{
				base.CurLevelPercentage = 0.8f;
			}
			else
			{
				base.CurLevelPercentage = Rand.Range(0.5f, 0.9f);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.lastNonStarvingTick = Find.TickManager.TicksGame;
			}
		}

		public override string GetTipString()
		{
			return string.Concat(new string[]
			{
				base.LabelCap,
				": ",
				base.CurLevelPercentage.ToStringPercent(),
				" (",
				this.CurLevel.ToString("0.##"),
				" / ",
				this.MaxLevel.ToString("0.##"),
				")\n",
				this.def.description
			});
		}

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (this.threshPercents == null)
			{
				this.threshPercents = new List<float>();
			}
			this.threshPercents.Clear();
			this.threshPercents.Add(this.PercentageThreshHungry);
			this.threshPercents.Add(this.PercentageThreshUrgentlyHungry);
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}
	}
}
