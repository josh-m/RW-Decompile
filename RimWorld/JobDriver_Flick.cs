using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Flick : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate
			{
				Designation designation = Find.DesignationManager.DesignationOn(this.<>f__this.TargetThingA, DesignationDefOf.Flick);
				return designation == null;
			});
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 15
			};
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<finalize>__1.actor;
					ThingWithComps thingWithComps = (ThingWithComps)actor.CurJob.targetA.Thing;
					for (int i = 0; i < thingWithComps.AllComps.Count; i++)
					{
						CompFlickable compFlickable = thingWithComps.AllComps[i] as CompFlickable;
						if (compFlickable != null && compFlickable.WantsFlick())
						{
							compFlickable.DoFlick();
						}
					}
					actor.records.Increment(RecordDefOf.SwitchesFlicked);
					Designation designation = Find.DesignationManager.DesignationOn(thingWithComps, DesignationDefOf.Flick);
					if (designation != null)
					{
						designation.Delete();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
