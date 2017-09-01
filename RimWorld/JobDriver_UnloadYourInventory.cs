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

		private int countToDrop = -1;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.countToDrop, "countToDrop", -1, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.Wait(10);
			yield return new Toil
			{
				initAction = delegate
				{
					if (!this.<>f__this.pawn.inventory.UnloadEverything)
					{
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						ThingStackPart firstUnloadableThing = this.<>f__this.pawn.inventory.FirstUnloadableThing;
						IntVec3 c;
						if (!StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, this.<>f__this.pawn, out c))
						{
							Thing thing;
							this.<>f__this.pawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out thing, null);
							this.<>f__this.EndJobWith(JobCondition.Succeeded);
						}
						else
						{
							this.<>f__this.CurJob.SetTarget(TargetIndex.A, firstUnloadableThing.Thing);
							this.<>f__this.CurJob.SetTarget(TargetIndex.B, c);
							this.<>f__this.countToDrop = firstUnloadableThing.Count;
						}
					}
				}
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
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
						this.<>f__this.pawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, this.<>f__this.countToDrop, out thing, null);
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						this.<>f__this.pawn.inventory.innerContainer.TryTransferToContainer(thing, this.<>f__this.pawn.carryTracker.innerContainer, this.<>f__this.countToDrop, out thing, true);
						this.<>f__this.CurJob.count = this.<>f__this.countToDrop;
						this.<>f__this.CurJob.SetTarget(TargetIndex.A, thing);
					}
					thing.SetForbidden(false, false);
				}
			};
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
		}
	}
}
