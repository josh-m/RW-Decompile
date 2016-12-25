using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public abstract class IngestionOutcomeDoer
	{
		public float chance = 1f;

		public void DoIngestionOutcome(Pawn pawn, Thing ingested)
		{
			if (Rand.Value < this.chance)
			{
				this.DoIngestionOutcomeSpecial(pawn, ingested);
			}
		}

		protected abstract void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested);

		[DebuggerHidden]
		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
		}
	}
}
