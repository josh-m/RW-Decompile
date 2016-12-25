using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class IngestionOutcomeDoer_GiveHediff : IngestionOutcomeDoer
	{
		public HediffDef hediffDef;

		public float severity = -1f;

		public ChemicalDef toleranceChemical;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			Hediff hediff = HediffMaker.MakeHediff(this.hediffDef, pawn, null);
			float initialSeverity;
			if (this.severity > 0f)
			{
				initialSeverity = this.severity;
			}
			else
			{
				initialSeverity = this.hediffDef.initialSeverity;
			}
			AddictionUtility.FactorDrugEffectForTolerance(pawn, this.toleranceChemical, ref initialSeverity);
			hediff.Severity = initialSeverity;
			pawn.health.AddHediff(hediff, null, null);
		}

		[DebuggerHidden]
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && this.chance >= 1f)
			{
				foreach (StatDrawEntry s in this.hediffDef.SpecialDisplayStats())
				{
					yield return s;
				}
			}
		}
	}
}
