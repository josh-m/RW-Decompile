using System;
using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_Rotten : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps == null)
			{
				return false;
			}
			CompRottable comp = thingWithComps.GetComp<CompRottable>();
			return comp != null && comp.Stage != RotStage.Fresh;
		}

		public override bool PotentiallyMatches(ThingDef def)
		{
			return def.HasComp(typeof(CompRottable));
		}
	}
}
