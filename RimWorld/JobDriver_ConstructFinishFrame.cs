using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ConstructFinishFrame : JobDriver
	{
		private const int JobEndInterval = 5000;

		private Frame Frame
		{
			get
			{
				return (Frame)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil build = new Toil();
			build.initAction = delegate
			{
				GenClamor.DoClamor(build.actor, 15f, ClamorType.Construction);
			};
			build.tickAction = delegate
			{
				Pawn actor = build.actor;
				Frame frame = this.$this.Frame;
				if (frame.resourceContainer.Count > 0)
				{
					actor.skills.Learn(SkillDefOf.Construction, 0.275f, false);
				}
				float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				float workToMake = frame.WorkToMake;
				if (actor.Faction == Faction.OfPlayer)
				{
					float statValue2 = actor.GetStatValue(StatDefOf.ConstructSuccessChance, true);
					if (Rand.Value < 1f - Mathf.Pow(statValue2, statValue / workToMake))
					{
						frame.FailConstruction(actor);
						this.$this.ReadyForNextToil();
						return;
					}
				}
				if (frame.def.entityDefToBuild is TerrainDef)
				{
					this.$this.Map.snowGrid.SetDepth(frame.Position, 0f);
				}
				frame.workDone += statValue;
				if (frame.workDone >= workToMake)
				{
					frame.CompleteConstruction(actor);
					this.$this.ReadyForNextToil();
					return;
				}
			};
			build.WithEffect(() => ((Frame)build.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).ConstructionEffect, TargetIndex.A);
			build.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			build.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			build.FailOn(() => !GenConstruct.CanConstruct(this.$this.Frame, this.$this.pawn, false));
			build.defaultCompleteMode = ToilCompleteMode.Delay;
			build.defaultDuration = 5000;
			yield return build;
		}
	}
}
