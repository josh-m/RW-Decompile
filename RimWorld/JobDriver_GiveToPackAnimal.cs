using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_GiveToPackAnimal : JobDriver
	{
		private const TargetIndex ItemInd = TargetIndex.A;

		private const TargetIndex AnimalInd = TargetIndex.B;

		private Thing Item
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Pawn Animal
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			Toil findNearestCarrier = this.FindNearestCarrierToil();
			yield return findNearestCarrier;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).JumpIf(() => !this.<>f__this.CanCarryAtLeastOne(this.<>f__this.Animal), findNearestCarrier);
			yield return this.GiveToCarrierAsMuchAsPossibleToil();
			yield return Toils_Jump.JumpIf(findNearestCarrier, () => this.<>f__this.pawn.carryTracker.CarriedThing != null);
		}

		private Toil FindNearestCarrierToil()
		{
			return new Toil
			{
				initAction = delegate
				{
					Pawn pawn = this.FindNearestCarrier();
					if (pawn == null)
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					}
					else
					{
						base.CurJob.SetTarget(TargetIndex.B, pawn);
					}
				}
			};
		}

		private Pawn FindNearestCarrier()
		{
			List<Pawn> list = base.Map.mapPawns.SpawnedPawnsInFaction(this.pawn.Faction);
			Pawn pawn = null;
			float num = -1f;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].RaceProps.packAnimal && this.CanCarryAtLeastOne(list[i]))
				{
					float num2 = list[i].Position.DistanceToSquared(this.pawn.Position);
					if (pawn == null || num2 < num)
					{
						pawn = list[i];
						num = num2;
					}
				}
			}
			return pawn;
		}

		private bool CanCarryAtLeastOne(Pawn carrier)
		{
			return !MassUtility.WillBeOverEncumberedAfterPickingUp(carrier, this.Item, 1);
		}

		private Toil GiveToCarrierAsMuchAsPossibleToil()
		{
			return new Toil
			{
				initAction = delegate
				{
					if (this.Item == null)
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					}
					else
					{
						int stackCount = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(this.Animal, this.Item), this.Item.stackCount);
						this.pawn.carryTracker.innerContainer.TransferToContainer(this.Item, this.Animal.inventory.innerContainer, stackCount);
					}
				}
			};
		}
	}
}
