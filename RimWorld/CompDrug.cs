using System;
using Verse;

namespace RimWorld
{
	public class CompDrug : ThingComp
	{
		public CompProperties_Drug Props
		{
			get
			{
				return (CompProperties_Drug)this.props;
			}
		}

		public override void PostIngested(Pawn ingester)
		{
			if (this.Props.Addictive && ingester.RaceProps.IsFlesh)
			{
				HediffDef addictionHediffDef = this.Props.chemical.addictionHediff;
				Hediff_Addiction hediff_Addiction = AddictionUtility.FindAddictionHediff(ingester, this.Props.chemical);
				Hediff hediff = AddictionUtility.FindToleranceHediff(ingester, this.Props.chemical);
				float num = (hediff == null) ? 0f : hediff.Severity;
				if (hediff_Addiction != null)
				{
					hediff_Addiction.Severity += this.Props.existingAddictionSeverityOffset;
				}
				else if (Rand.Value < this.Props.addictiveness && num >= this.Props.minToleranceToAddict)
				{
					ingester.health.AddHediff(addictionHediffDef, null, null);
					if (PawnUtility.ShouldSendNotificationAbout(ingester))
					{
						Find.LetterStack.ReceiveLetter("LetterLabelNewlyAddicted".Translate(new object[]
						{
							this.Props.chemical.label
						}).CapitalizeFirst(), "LetterNewlyAddicted".Translate(new object[]
						{
							ingester.LabelShort,
							this.Props.chemical.label
						}).AdjustedFor(ingester).CapitalizeFirst(), LetterType.BadNonUrgent, ingester, null);
					}
					AddictionUtility.CheckDrugAddictionTeachOpportunity(ingester);
				}
				if (addictionHediffDef.causesNeed != null)
				{
					Need need = ingester.needs.AllNeeds.Find((Need x) => x.def == addictionHediffDef.causesNeed);
					if (need != null)
					{
						float needLevelOffset = this.Props.needLevelOffset;
						AddictionUtility.FactorDrugEffectForTolerance(ingester, this.Props.chemical, ref needLevelOffset);
						need.CurLevel += needLevelOffset;
					}
				}
				float randomInRange = this.Props.overdoseSeverityOffset.RandomInRange;
				if (randomInRange > 0f)
				{
					HealthUtility.AdjustSeverity(ingester, HediffDefOf.DrugOverdose, randomInRange);
				}
			}
			if (this.Props.isCombatEnhancingDrug && !ingester.Dead)
			{
				ingester.mindState.lastTakeCombatEnancingDrugTick = Find.TickManager.TicksGame;
			}
			if (ingester.drugs != null)
			{
				ingester.drugs.Notify_DrugIngested(this.parent);
			}
		}
	}
}
