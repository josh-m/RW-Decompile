using RimWorld;
using System;

namespace Verse
{
	public class Hediff_Alcohol : HediffWithComps
	{
		private const int HangoverCheckInterval = 300;

		public override void Tick()
		{
			base.Tick();
			if (this.CurStageIndex >= 3 && this.pawn.IsHashIntervalTick(300) && this.HangoverSusceptible(this.pawn))
			{
				Hediff hediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hangover);
				if (hediff != null)
				{
					hediff.Severity = 1f;
				}
				else
				{
					hediff = HediffMaker.MakeHediff(HediffDefOf.Hangover, this.pawn, null);
					hediff.Severity = 1f;
					this.pawn.health.AddHediff(hediff, null, null);
				}
			}
		}

		private bool HangoverSusceptible(Pawn pawn)
		{
			return true;
		}
	}
}
