using System;
using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_Smeltable : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			return this.CanEverMatch(t.def) && t.Smeltable;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.smeltable;
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			return def.smeltable && !def.MadeFromStuff;
		}
	}
}
