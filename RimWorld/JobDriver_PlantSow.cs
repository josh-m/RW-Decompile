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
				return (Plant)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.sowWorkDone, "sowWorkDone", 0f, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => GenPlant.AdjacentSowBlocker(this.<>f__this.CurJob.plantDefToSow, this.<>f__this.TargetA.Cell) != null);
			Toil sowToil = new Toil();
			sowToil.initAction = delegate
			{
				this.<>f__this.TargetThingA = GenSpawn.Spawn(this.<>f__this.CurJob.plantDefToSow, this.<>f__this.TargetLocA);
				Plant plant = (Plant)this.<>f__this.TargetThingA;
				plant.Growth = 0f;
				plant.sown = true;
			};
			sowToil.tickAction = delegate
			{
				Pawn actor = this.<sowToil>__0.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Growing, 0.11f);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				float num = statValue;
				Plant plant = this.<>f__this.Plant;
				if (plant.LifeStage != PlantLifeStage.Sowing)
				{
					Log.Error(this.<>f__this + " getting sowing work while not in Sowing life stage.");
				}
				this.<>f__this.sowWorkDone += num;
				if (this.<>f__this.sowWorkDone >= plant.def.plant.sowWork)
				{
					plant.Growth = 0.05f;
					Find.MapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
					actor.records.Increment(RecordDefOf.PlantsSown);
					this.<>f__this.ReadyForNextToil();
					return;
				}
			};
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.WithEffect("Sow", TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => this.<>f__this.sowWorkDone / this.<>f__this.Plant.def.plant.sowWork, true, -0.5f);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (this.<>f__this.TargetThingA != null)
				{
					Plant plant = (Plant)this.<sowToil>__0.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (this.<>f__this.sowWorkDone < plant.def.plant.sowWork && !this.<>f__this.TargetThingA.Destroyed)
					{
						this.<>f__this.TargetThingA.Destroy(DestroyMode.Vanish);
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
						this.<>f__this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.GreenThumbHappy, null);
					}
				};
			}
		}
	}
}
