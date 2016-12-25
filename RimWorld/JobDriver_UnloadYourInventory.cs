using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UnloadYourInventory : JobDriver
	{
		private const TargetIndex ItemToHaulInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		private const int UnloadDuration = 10;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.Wait(10);
			yield return new Toil
			{
				initAction = delegate
				{
					if (!this.<>f__this.pawn.inventory.UnloadEverything || this.<>f__this.pawn.inventory.innerContainer.Count == 0)
					{
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						Thing thing = this.<>f__this.pawn.inventory.innerContainer.RandomElement<Thing>();
						IntVec3 c;
						if (!StoreUtility.TryFindStoreCellNearColonyDesperate(thing, this.<>f__this.pawn, out c))
						{
							this.<>f__this.pawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing, null);
							this.<>f__this.EndJobWith(JobCondition.Succeeded);
						}
						else
						{
							this.<>f__this.CurJob.SetTarget(TargetIndex.A, thing);
							this.<>f__this.CurJob.SetTarget(TargetIndex.B, c);
						}
					}
				}
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.<>f__this.CurJob.GetTarget(TargetIndex.A).Thing;
					if (thing == null || !this.<>f__this.pawn.inventory.innerContainer.Contains(thing))
					{
						this.<>f__this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					if (!this.<>f__this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !thing.def.EverStoreable)
					{
						this.<>f__this.pawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing, null);
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						this.<>f__this.pawn.inventory.innerContainer.TransferToContainer(thing, this.<>f__this.pawn.carryTracker.innerContainer, thing.stackCount, out thing);
						this.<>f__this.CurJob.count = thing.stackCount;
						this.<>f__this.CurJob.SetTarget(TargetIndex.A, thing);
					}
					thing.SetForbidden(false, false);
					if (this.<>f__this.pawn.inventory.innerContainer.Count == 0)
					{
						this.<>f__this.pawn.inventory.UnloadEverything = false;
					}
				}
			};
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
		}
	}
}
