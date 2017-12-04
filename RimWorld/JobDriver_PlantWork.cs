using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public abstract class JobDriver_PlantWork : JobDriver
	{
		private float workDone;

		protected float xpPerTick;

		protected const TargetIndex PlantInd = TargetIndex.A;

		protected Plant Plant
		{
			get
			{
				return (Plant)this.job.targetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
			if (target.IsValid && !this.pawn.Reserve(target, this.job, 1, -1, null))
			{
				return false;
			}
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.Init();
			yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
			Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
			yield return initExtractTargetFromQueue;
			yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A, true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue);
			Toil cut = new Toil();
			cut.tickAction = delegate
			{
				Pawn actor = cut.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Growing, this.$this.xpPerTick, false);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				float num = statValue;
				Plant plant = this.$this.Plant;
				this.$this.workDone += num;
				if (this.$this.workDone >= plant.def.plant.harvestWork)
				{
					if (plant.def.plant.harvestedThingDef != null)
					{
						if (actor.RaceProps.Humanlike && plant.def.plant.harvestFailable && Rand.Value > actor.GetStatValue(StatDefOf.PlantHarvestYield, true))
						{
							Vector3 loc = (this.$this.pawn.DrawPos + plant.DrawPos) / 2f;
							MoteMaker.ThrowText(loc, this.$this.Map, "TextMote_HarvestFailed".Translate(), 3.65f);
						}
						else
						{
							int num2 = plant.YieldNow();
							if (num2 > 0)
							{
								Thing thing = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef, null);
								thing.stackCount = num2;
								if (actor.Faction != Faction.OfPlayer)
								{
									thing.SetForbidden(true, true);
								}
								GenPlace.TryPlaceThing(thing, actor.Position, this.$this.Map, ThingPlaceMode.Near, null);
								actor.records.Increment(RecordDefOf.PlantsHarvested);
							}
						}
					}
					plant.def.plant.soundHarvestFinish.PlayOneShot(actor);
					plant.PlantCollected();
					this.$this.workDone = 0f;
					this.$this.ReadyForNextToil();
					return;
				}
			};
			cut.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			cut.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			cut.defaultCompleteMode = ToilCompleteMode.Never;
			cut.WithEffect(EffecterDefOf.Harvest, TargetIndex.A);
			cut.WithProgressBar(TargetIndex.A, () => this.$this.workDone / this.$this.Plant.def.plant.harvestWork, true, -0.5f);
			cut.PlaySustainerOrSound(() => this.$this.Plant.def.plant.soundHarvesting);
			yield return cut;
			Toil plantWorkDoneToil = this.PlantWorkDoneToil();
			if (plantWorkDoneToil != null)
			{
				yield return plantWorkDoneToil;
			}
			yield return Toils_Jump.Jump(initExtractTargetFromQueue);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.workDone, "workDone", 0f, false);
		}

		protected virtual void Init()
		{
		}

		protected virtual Toil PlantWorkDoneToil()
		{
			return null;
		}
	}
}
