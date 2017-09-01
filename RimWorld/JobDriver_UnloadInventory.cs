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
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(10);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn otherPawn = this.<>f__this.OtherPawn;
					if (!otherPawn.inventory.UnloadEverything)
					{
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						ThingStackPart firstUnloadableThing = otherPawn.inventory.FirstUnloadableThing;
						IntVec3 c;
						if (!firstUnloadableThing.Thing.def.EverStoreable || !this.<>f__this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, this.<>f__this.pawn, out c))
						{
							Thing thing;
							otherPawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out thing, null);
							this.<>f__this.EndJobWith(JobCondition.Succeeded);
							if (thing != null)
							{
								thing.SetForbidden(false, false);
							}
						}
						else
						{
							Thing thing2;
							otherPawn.inventory.innerContainer.TryTransferToContainer(firstUnloadableThing.Thing, this.<>f__this.pawn.carryTracker.innerContainer, firstUnloadableThing.Count, out thing2, true);
							this.<>f__this.CurJob.count = thing2.stackCount;
							this.<>f__this.CurJob.SetTarget(TargetIndex.B, thing2);
							this.<>f__this.CurJob.SetTarget(TargetIndex.C, c);
							firstUnloadableThing.Thing.SetForbidden(false, false);
						}
					}
				}
			};
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
		}
	}
}
