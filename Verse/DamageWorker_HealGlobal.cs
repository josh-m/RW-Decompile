using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DamageWorker_HealGlobal : DamageWorker
	{
		public override float Apply(DamageInfo dinfo, Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				if (dinfo.Part.Value.HealHediff.NullOrEmpty<HediffDef>())
				{
					Log.Warning("Tried to heal global injury with no specified health diffs to heal.");
					return 0f;
				}
				IEnumerable<HediffDef> source = from x in dinfo.Part.Value.HealHediff
				where pawn.health.hediffSet.GetHediffs<Hediff>().Any((Hediff y) => y.def == x)
				select x;
				if (source.Any<HediffDef>())
				{
					HediffDef globalDef = source.RandomElement<HediffDef>();
					Hediff hediff = (from x in pawn.health.hediffSet.hediffs
					where x.def == globalDef
					select x).FirstOrDefault<Hediff>();
					if (hediff != null)
					{
						pawn.health.HealHediff(hediff, dinfo.Amount);
					}
				}
			}
			return 0f;
		}
	}
}
