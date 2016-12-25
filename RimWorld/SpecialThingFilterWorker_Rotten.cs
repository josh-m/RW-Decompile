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
			return comp != null && !((CompProperties_Rottable)comp.props).rotDestroys && comp.Stage != RotStage.Fresh;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			CompProperties_Rottable compProperties = def.GetCompProperties<CompProperties_Rottable>();
			return compProperties != null && !compProperties.rotDestroys;
		}
	}
}
