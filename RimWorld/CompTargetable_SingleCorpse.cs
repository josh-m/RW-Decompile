using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompTargetable_SingleCorpse : CompTargetable
	{
		protected override bool PlayerChoosesTarget
		{
			get
			{
				return true;
			}
		}

		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = ((TargetInfo x) => x.Thing is Corpse && base.BaseTargetValidator(x.Thing))
			};
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
		}
	}
}
