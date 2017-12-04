using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_CastVerbOnce : JobDriver
	{
		public override string GetReport()
		{
			string text;
			if (base.TargetA.HasThing)
			{
				text = base.TargetThingA.LabelCap;
			}
			else
			{
				text = "AreaLower".Translate();
			}
			return "UsingVerb".Translate(new object[]
			{
				this.job.verbToUse.verbProps.label,
				text
			});
		}

		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Combat.GotoCastPosition(TargetIndex.A, false);
			yield return Toils_Combat.CastVerb(TargetIndex.A, true);
		}
	}
}
