using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_ForcedHediff : ScenPart_PawnModifier
	{
		private HediffDef hediff;

		private FloatRange severityRange;

		private bool hideOffMap;

		private float MaxSeverity
		{
			get
			{
				return (this.hediff.lethalSeverity <= 0f) ? 1f : (this.hediff.lethalSeverity * 0.99f);
			}
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 31f);
			if (Widgets.ButtonText(scenPartRect.TopPartPixels(ScenPart.RowHeight), this.hediff.LabelCap, true, false, true))
			{
				FloatMenuUtility.MakeMenu<HediffDef>(this.PossibleHediffs(), (HediffDef hd) => hd.LabelCap, (HediffDef hd) => delegate
				{
					this.hediff = hd;
					if (this.severityRange.max > this.MaxSeverity)
					{
						this.severityRange.max = this.MaxSeverity;
					}
					if (this.severityRange.min > this.MaxSeverity)
					{
						this.severityRange.min = this.MaxSeverity;
					}
				});
			}
			Widgets.FloatRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref this.severityRange, 0f, this.MaxSeverity, "ConfigurableSeverity", ToStringStyle.FloatTwo);
			base.DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
		}

		private IEnumerable<HediffDef> PossibleHediffs()
		{
			return from x in DefDatabase<HediffDef>.AllDefsListForReading
			where x.scenarioCanAdd
			select x;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<HediffDef>(ref this.hediff, "hediff");
			Scribe_Values.LookValue<FloatRange>(ref this.severityRange, "severityRange", default(FloatRange), false);
			Scribe_Values.LookValue<bool>(ref this.hideOffMap, "hideOffMap", false, false);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_PawnsHaveHediff".Translate(new object[]
			{
				this.context.ToStringHuman(),
				this.chance.ToStringPercent(),
				this.hediff.label
			}).CapitalizeFirst();
		}

		public override void Randomize()
		{
			base.Randomize();
			this.hediff = this.PossibleHediffs().RandomElement<HediffDef>();
			this.severityRange.max = Rand.Range(this.MaxSeverity * 0.2f, this.MaxSeverity * 0.95f);
			this.severityRange.min = this.severityRange.max * Rand.Range(0f, 0.95f);
			this.hideOffMap = false;
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_ForcedHediff scenPart_ForcedHediff = other as ScenPart_ForcedHediff;
			if (scenPart_ForcedHediff != null && this.hediff == scenPart_ForcedHediff.hediff)
			{
				this.chance = GenMath.ChanceEitherHappens(this.chance, scenPart_ForcedHediff.chance);
				return true;
			}
			return false;
		}

		protected override void ModifyPawn(Pawn p)
		{
			if (Rand.Value < this.chance)
			{
				Hediff hediff = HediffMaker.MakeHediff(this.hediff, p, null);
				hediff.Severity = this.severityRange.RandomInRange;
				hediff.hiddenOffMap = this.hideOffMap;
				p.health.AddHediff(hediff, null, null);
			}
		}
	}
}
