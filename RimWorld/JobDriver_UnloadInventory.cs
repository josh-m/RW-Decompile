using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UnloadInventory : JobDriver
	{
		private const TargetIndex OtherPawnInd = TargetIndex.A;

		private const TargetIndex ItemToHaulInd = TargetIndex.B;

		private const TargetIndex StoreCellInd = TargetIndex.C;

		private const int UnloadDuration = 10;

		private Pawn OtherPawn
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(10);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn otherPawn = this.<>f__this.OtherPawn;
					if (!otherPawn.inventory.UnloadEverything || otherPawn.inventory.innerContainer.Count == 0)
					{
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						Thing thing = otherPawn.inventory.innerContainer.RandomElement<Thing>();
						IntVec3 c;
						if (!thing.def.EverStoreable || !this.<>f__this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !StoreUtility.TryFindStoreCellNearColonyDesperate(thing, this.<>f__this.pawn, out c))
						{
							otherPawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing, null);
							this.<>f__this.EndJobWith(JobCondition.Succeeded);
						}
						else
						{
							otherPawn.inventory.innerContainer.TransferToContainer(thing, this.<>f__this.pawn.carryTracker.innerContainer, thing.stackCount, out thing);
							this.<>f__this.CurJob.count = thing.stackCount;
							this.<>f__this.CurJob.SetTarget(TargetIndex.B, thing);
							this.<>f__this.CurJob.SetTarget(TargetIndex.C, c);
						}
						thing.SetForbidden(false, false);
						if (otherPawn.inventory.innerContainer.Count == 0)
						{
							otherPawn.inventory.UnloadEverything = false;
						}
					}
				}
			};
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
		}
	}
}
