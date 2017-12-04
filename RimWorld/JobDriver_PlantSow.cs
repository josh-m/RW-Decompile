using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlantSow : JobDriver
	{
		private float sowWorkDone;

		private Plant Plant
		{
			get
			{
				return (Plant)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.sowWorkDone, "sowWorkDone", 0f, false);
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => GenPlant.AdjacentSowBlocker(this.$this.job.plantDefToSow, this.$this.TargetA.Cell, this.$this.Map) != null).FailOn(() => !this.$this.job.plantDefToSow.CanEverPlantAt(this.$this.TargetLocA, this.$this.Map));
			Toil sowToil = new Toil();
			sowToil.initAction = delegate
			{
				this.$this.TargetThingA = GenSpawn.Spawn(this.$this.job.plantDefToSow, this.$this.TargetLocA, this.$this.Map);
				this.$this.pawn.Reserve(this.$this.TargetThingA, sowToil.actor.CurJob, 1, -1, null);
				Plant plant = (Plant)this.$this.TargetThingA;
				plant.Growth = 0f;
				plant.sown = true;
			};
			sowToil.tickAction = delegate
			{
				Pawn actor = sowToil.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Growing, 0.11f, false);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				float num = statValue;
				Plant plant = this.$this.Plant;
				if (plant.LifeStage != PlantLifeStage.Sowing)
				{
					Log.Error(this.$this + " getting sowing work while not in Sowing life stage.");
				}
				this.$this.sowWorkDone += num;
				if (this.$this.sowWorkDone >= plant.def.plant.sowWork)
				{
					plant.Growth = 0.05f;
					this.$this.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
					actor.records.Increment(RecordDefOf.PlantsSown);
					this.$this.ReadyForNextToil();
					return;
				}
			};
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => this.$this.sowWorkDone / this.$this.Plant.def.plant.sowWork, true, -0.5f);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (this.$this.TargetThingA != null)
				{
					Plant plant = (Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (this.$this.sowWorkDone < plant.def.plant.sowWork && !this.$this.TargetThingA.Destroyed)
					{
						this.$this.TargetThingA.Destroy(DestroyMode.Vanish);
					}
				}
			});
			yield return sowToil;
			if (this.pawn.story.traits.HasTrait(TraitDefOf.GreenThumb))
			{
				yield return new Toil
				{
					initAction = delegate
					{
						this.$this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GreenThumbHappy, null);
					}
				};
			}
		}
	}
}
