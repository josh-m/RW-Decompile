using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UnloadYourInventory : JobDriver
	{
		private int countToDrop = -1;

		private const TargetIndex ItemToHaulInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		private const int UnloadDuration = 10;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.countToDrop, "countToDrop", -1, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.Wait(10, TargetIndex.None);
			yield return new Toil
			{
				initAction = delegate
				{
					if (!this.$this.pawn.inventory.UnloadEverything)
					{
						this.$this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						ThingCount firstUnloadableThing = this.$this.pawn.inventory.FirstUnloadableThing;
						IntVec3 c;
						if (!StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, this.$this.pawn, out c))
						{
							Thing thing;
							this.$this.pawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out thing, null, null);
							this.$this.EndJobWith(JobCondition.Succeeded);
						}
						else
						{
							this.$this.job.SetTarget(TargetIndex.A, firstUnloadableThing.Thing);
							this.$this.job.SetTarget(TargetIndex.B, c);
							this.$this.countToDrop = firstUnloadableThing.Count;
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
					Thing thing = this.$this.job.GetTarget(TargetIndex.A).Thing;
					if (thing == null || !this.$this.pawn.inventory.innerContainer.Contains(thing))
					{
						this.$this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					if (!this.$this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !thing.def.EverStorable(false))
					{
						this.$this.pawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, this.$this.countToDrop, out thing, null, null);
						this.$this.EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						this.$this.pawn.inventory.innerContainer.TryTransferToContainer(thing, this.$this.pawn.carryTracker.innerContainer, this.$this.countToDrop, out thing, true);
						this.$this.job.count = this.$this.countToDrop;
						this.$this.job.SetTarget(TargetIndex.A, thing);
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
