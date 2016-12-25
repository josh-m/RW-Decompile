using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_StatFactor : ScenPart
	{
		private StatDef stat;

		private float factor;

		private string factorBuf;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<StatDef>(ref this.stat, "stat");
			Scribe_Values.LookValue<float>(ref this.factor, "factor", 0f, false);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			Rect rect = scenPartRect.TopHalf();
			if (Widgets.ButtonText(rect, this.stat.LabelCap, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (StatDef current in DefDatabase<StatDef>.AllDefs)
				{
					StatDef localSd = current;
					list.Add(new FloatMenuOption(localSd.LabelCap, delegate
					{
						this.stat = localSd;
					}, MenuOptionPriority.Medium, null, null, 0f, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Rect rect2 = scenPartRect.BottomHalf();
			Rect rect3 = rect2.LeftHalf().Rounded();
			Rect rect4 = rect2.RightHalf().Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect3, "multiplier".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldPercent(rect4, ref this.factor, ref this.factorBuf, 0f, 100f);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_StatFactor".Translate(new object[]
			{
				this.stat.label,
				this.factor.ToStringPercent()
			});
		}

		public override void Randomize()
		{
			this.stat = this.RandomizableStats().RandomElement<StatDef>();
			this.factor = GenMath.RoundedHundredth(Rand.Range(0.1f, 3f));
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_StatFactor scenPart_StatFactor = other as ScenPart_StatFactor;
			if (scenPart_StatFactor != null && scenPart_StatFactor.stat == this.stat)
			{
				this.factor *= scenPart_StatFactor.factor;
				return true;
			}
			return false;
		}

		public float GetStatFactor(StatDef stat)
		{
			if (stat == this.stat)
			{
				return this.factor;
			}
			return 1f;
		}

		[DebuggerHidden]
		private IEnumerable<StatDef> RandomizableStats()
		{
			yield return StatDefOf.ComfyTemperatureMax;
			yield return StatDefOf.ComfyTemperatureMin;
			yield return StatDefOf.ConstructFailChance;
			yield return StatDefOf.ConstructionSpeed;
			yield return StatDefOf.DeteriorationRate;
			yield return StatDefOf.Flammability;
			yield return StatDefOf.GlobalLearningFactor;
			yield return StatDefOf.HarvestFailChance;
			yield return StatDefOf.HealingSpeed;
			yield return StatDefOf.ImmunityGainSpeed;
			yield return StatDefOf.MarketValue;
			yield return StatDefOf.MaxHitPoints;
			yield return StatDefOf.MentalBreakThreshold;
			yield return StatDefOf.MiningSpeed;
			yield return StatDefOf.MoveSpeed;
			yield return StatDefOf.PsychicSensitivity;
			yield return StatDefOf.ResearchSpeed;
			yield return StatDefOf.ShootingAccuracy;
			yield return StatDefOf.SurgerySuccessChance;
			yield return StatDefOf.RecruitPrisonerChance;
			yield return StatDefOf.TameAnimalChance;
			yield return StatDefOf.TrainAnimalChance;
			yield return StatDefOf.MeleeWeapon_DamageAmount;
			yield return StatDefOf.RangedWeapon_Cooldown;
			yield return StatDefOf.WorkSpeedGlobal;
			yield return StatDefOf.WorkToMake;
		}
	}
}
