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

		protected Plant Plant
		{
			get
			{
				return (Plant)base.CurJob.targetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.Init();
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil cutToil = new Toil();
			cutToil.tickAction = delegate
			{
				Pawn actor = this.<cutToil>__0.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Growing, this.<>f__this.xpPerTick);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				float num = statValue;
				Plant plant = this.<>f__this.Plant;
				this.<>f__this.workDone += num;
				if (this.<>f__this.workDone >= plant.def.plant.harvestWork)
				{
					if (plant.def.plant.harvestedThingDef != null)
					{
						if (actor.RaceProps.Humanlike && plant.def.plant.harvestFailable && Rand.Value < actor.GetStatValue(StatDefOf.HarvestFailChance, true))
						{
							Vector3 loc = (this.<>f__this.pawn.DrawPos + plant.DrawPos) / 2f;
							MoteMaker.ThrowText(loc, "HarvestFailed".Translate(), 3.65f);
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
								GenPlace.TryPlaceThing(thing, actor.Position, ThingPlaceMode.Near, null);
								actor.records.Increment(RecordDefOf.PlantsHarvested);
							}
						}
					}
					plant.def.plant.soundHarvestFinish.PlayOneShot(actor);
					plant.PlantCollected();
					this.<>f__this.workDone = 0f;
					this.<>f__this.ReadyForNextToil();
					return;
				}
			};
			cutToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			cutToil.defaultCompleteMode = ToilCompleteMode.Never;
			cutToil.WithEffect("Harvest", TargetIndex.A);
			cutToil.WithProgressBar(TargetIndex.A, () => this.<>f__this.workDone / this.<>f__this.Plant.def.plant.harvestWork, true, -0.5f);
			cutToil.PlaySustainerOrSound(() => this.<>f__this.Plant.def.plant.soundHarvesting);
			yield return cutToil;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.workDone, "cutWorkDone", 0f, false);
		}

		protected virtual void Init()
		{
		}
	}
}
