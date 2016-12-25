using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ConstructFinishFrame : JobDriver
	{
		private const int JobEndInterval = 5000;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil build = new Toil();
			build.tickAction = delegate
			{
				Pawn actor = this.<build>__0.actor;
				Frame frame = (Frame)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				actor.skills.Learn(SkillDefOf.Construction, 0.275f, false);
				float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				float workToMake = frame.WorkToMake;
				float statValue2 = actor.GetStatValue(StatDefOf.ConstructFailChance, true);
				if (statValue2 > 0f)
				{
					float mtb = workToMake / statValue2;
					if (Rand.MTBEventOccurs(mtb, 1f, statValue))
					{
						frame.FailConstruction(actor);
						this.<>f__this.ReadyForNextToil();
						return;
					}
				}
				if (frame.def.entityDefToBuild is TerrainDef)
				{
					this.<>f__this.Map.snowGrid.SetDepth(frame.Position, 0f);
				}
				frame.workDone += statValue;
				if (workToMake - frame.workDone <= 0f)
				{
					frame.CompleteConstruction(actor);
					this.<>f__this.ReadyForNextToil();
					return;
				}
			};
			build.WithEffect(() => ((Frame)this.<build>__0.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).ConstructionEffect, TargetIndex.A);
			build.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			build.defaultCompleteMode = ToilCompleteMode.Delay;
			build.defaultDuration = 5000;
			yield return build;
		}
	}
}
