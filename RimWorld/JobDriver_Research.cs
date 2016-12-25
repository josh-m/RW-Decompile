using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Research : JobDriver
	{
		private const int JobEndInterval = 4000;

		private ResearchProjectDef Project
		{
			get
			{
				return Find.ResearchManager.currentProj;
			}
		}

		private Building_ResearchBench ResearchBench
		{
			get
			{
				return (Building_ResearchBench)base.TargetThingA;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil research = new Toil();
			research.tickAction = delegate
			{
				Pawn actor = this.<research>__0.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				num *= this.<>f__this.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
				Find.ResearchManager.ResearchPerformed(num, actor);
				actor.skills.Learn(SkillDefOf.Research, 0.11f, false);
				actor.GainComfortFromCellIfPossible();
			};
			research.FailOn(() => this.<>f__this.Project == null);
			research.FailOn(() => !this.<>f__this.Project.CanBeResearchedAt(this.<>f__this.ResearchBench, false));
			research.WithEffect(EffecterDefOf.Research, TargetIndex.A);
			research.WithProgressBar(TargetIndex.A, delegate
			{
				ResearchProjectDef project = this.<>f__this.Project;
				if (project == null)
				{
					return 0f;
				}
				return project.ProgressPercent;
			}, false, -0.5f);
			research.defaultCompleteMode = ToilCompleteMode.Delay;
			research.defaultDuration = 4000;
			yield return research;
			yield return Toils_General.Wait(2);
		}
	}
}
