using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TakeBeerOutOfFermentingBarrel : JobDriver
	{
		private const TargetIndex BarrelInd = TargetIndex.A;

		private const TargetIndex BeerToHaulInd = TargetIndex.B;

		private const TargetIndex StorageCellInd = TargetIndex.C;

		private const int Duration = 200;

		protected Building_FermentingBarrel Barrel
		{
			get
			{
				return (Building_FermentingBarrel)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing Beer
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOn(() => !this.<>f__this.Barrel.Fermented).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.<>f__this.Barrel.TakeOutBeer();
					GenPlace.TryPlaceThing(thing, this.<>f__this.pawn.Position, ThingPlaceMode.Near, null);
					StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(thing.Position, thing);
					IntVec3 vec;
					if (StoreUtility.TryFindBestBetterStoreCellFor(thing, this.<>f__this.pawn, currentPriority, this.<>f__this.pawn.Faction, out vec, true))
					{
						this.<>f__this.CurJob.SetTarget(TargetIndex.C, vec);
						this.<>f__this.CurJob.SetTarget(TargetIndex.B, thing);
						this.<>f__this.CurJob.maxNumToCarry = thing.stackCount;
					}
					else
					{
						this.<>f__this.EndJobWith(JobCondition.Incompletable);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
		}
	}
}
