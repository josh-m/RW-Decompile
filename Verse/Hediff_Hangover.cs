using RimWorld;
using System;

namespace Verse
{
	public class Hediff_Hangover : HediffWithComps
	{
		public override bool Visible
		{
			get
			{
				return !this.pawn.health.hediffSet.HasHediff(HediffDefOf.AlcoholHigh) && base.Visible;
			}
		}
	}
}
