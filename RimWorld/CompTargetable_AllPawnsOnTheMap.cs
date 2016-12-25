using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompTargetable_AllPawnsOnTheMap : CompTargetable
	{
		protected override bool PlayerChoosesTarget
		{
			get
			{
				return false;
			}
		}

		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				validator = ((TargetInfo x) => base.BaseTargetValidator(x.Thing))
			};
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			if (this.parent.MapHeld != null)
			{
				TargetingParameters tp = this.GetTargetingParameters();
				foreach (Pawn p in this.parent.MapHeld.mapPawns.AllPawnsSpawned)
				{
					if (tp.CanTarget(p))
					{
						yield return p;
					}
				}
			}
		}
	}
}
