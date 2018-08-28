using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_SubTree : ThingSetMaker
	{
		public ThingSetMakerDef def;

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			return this.def.root.CanGenerate(parms);
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			outThings.AddRange(this.def.root.Generate(parms));
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return this.def.root.AllGeneratableThingsDebug(parms);
		}
	}
}
