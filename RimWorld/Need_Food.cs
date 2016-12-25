using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Food : Need
	{
		private const float BaseFoodFallPerTick = 2.66666666E-05f;

		private const int TicksBetweenStarveDamage = 15000;

		private const float MalnutritionDamAmount = 0.066666f;

		private int tickToStarveDamage;

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
				return this.pawn.RaceProps.FoodLevelPercentageWantEat * 0.45f;
			}
		}

		public float PercentageThreshHungry
		{
			get
			{
				return this.pawn.RaceProps.FoodLevelPercentageWantEat * 0.9f;
			}
		}

		public HungerCategory CurCategory
		{
			get
			{
				if (base.CurLevelPercentage < 0.01f)
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
				switch (this.CurCategory)
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
			Scribe_Values.LookValue<int>(ref this.tickToStarveDamage, "ticksToNextStarveDamage", 0, false);
			Scribe_Values.LookValue<int>(ref this.lastNonStarvingTick, "lastNonStarvingTick", -99999, false);
		}

		public override void NeedInterval()
		{
			this.CurLevel -= this.FoodFallPerTick * 150f;
			if (!this.Starving)
			{
				this.lastNonStarvingTick = Find.TickManager.TicksGame;
			}
			this.tickToStarveDamage -= 150;
			if (this.tickToStarveDamage <= 0)
			{
				if (this.Starving)
				{
					HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.Malnutrition, 0.066666f);
				}
				else
				{
					HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.Malnutrition, -0.066666f);
				}
				this.tickToStarveDamage = 15000;
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
			if (Current.ProgramState == ProgramState.MapPlaying)
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

		public override void DrawOnGUI(Rect rect)
		{
			if (this.threshPercents == null)
			{
				this.threshPercents = new List<float>();
			}
			this.threshPercents.Clear();
			this.threshPercents.Add(this.PercentageThreshHungry);
			this.threshPercents.Add(this.PercentageThreshUrgentlyHungry);
			base.DrawOnGUI(rect);
		}
	}
}
