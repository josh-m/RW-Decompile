using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherItems : JobDriver
	{
		private const TargetIndex ToHaulInd = TargetIndex.A;

		private const TargetIndex CarrierInd = TargetIndex.B;

		private const int PlaceInInventoryDuration = 25;

		public Thing ToHaul
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Pawn Carrier
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		private List<TransferableOneWay> Transferables
		{
			get
			{
				return ((LordJob_FormAndSendCaravan)this.pawn.GetLord().LordJob).transferables;
			}
		}

		private TransferableOneWay Transferable
		{
			get
			{
				Thing toHaul = this.ToHaul;
				TransferableOneWay transferableOneWay = this.Transferables.Find((TransferableOneWay x) => x.things.Contains(toHaul));
				if (transferableOneWay != null)
				{
					return transferableOneWay;
				}
				transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(toHaul, this.Transferables);
				if (transferableOneWay != null)
				{
					return transferableOneWay;
				}
				throw new InvalidOperationException("Could not find any matching transferable.");
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil reserve = Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return reserve;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return this.DetermineNumToHaul();
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			yield return this.AddCarriedThingToTransferables();
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserve, TargetIndex.A, TargetIndex.None, true, (Thing x) => this.<>f__this.Transferable.things.Contains(x) || TransferableUtility.TransferAsOne(this.<>f__this.ToHaul, x));
			Toil findCarrier = this.FindCarrier();
			yield return findCarrier;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).JumpIf(() => !this.<>f__this.IsUsableCarrier(this.<>f__this.Carrier), findCarrier);
			yield return Toils_General.Wait(25).JumpIf(() => !this.<>f__this.IsUsableCarrier(this.<>f__this.Carrier), findCarrier).WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
			yield return this.PlaceTargetInCarrierInventory();
		}

		private Toil DetermineNumToHaul()
		{
			return new Toil
			{
				initAction = delegate
				{
					int num = GatherItemsForCaravanUtility.CountLeftToTransfer(this.pawn, this.Transferable);
					if (num <= 0)
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
					else
					{
						base.CurJob.count = num;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		private Toil AddCarriedThingToTransferables()
		{
			return new Toil
			{
				initAction = delegate
				{
					TransferableOneWay transferable = this.Transferable;
					if (!transferable.things.Contains(this.pawn.carryTracker.CarriedThing))
					{
						transferable.things.Add(this.pawn.carryTracker.CarriedThing);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		private Toil FindCarrier()
		{
			return new Toil
			{
				initAction = delegate
				{
					Pawn pawn = this.FindBestCarrier(true);
					if (pawn == null)
					{
						if (!MassUtility.IsOverEncumbered(this.pawn))
						{
							pawn = this.pawn;
						}
						else
						{
							pawn = this.FindBestCarrier(false);
							if (pawn == null)
							{
								pawn = this.pawn;
							}
						}
					}
					base.CurJob.SetTarget(TargetIndex.B, pawn);
				}
			};
		}

		private Toil PlaceTargetInCarrierInventory()
		{
			return new Toil
			{
				initAction = delegate
				{
					Pawn_CarryTracker carryTracker = this.pawn.carryTracker;
					Thing carriedThing = carryTracker.CarriedThing;
					this.Transferable.countToTransfer = Mathf.Max(this.Transferable.countToTransfer - carriedThing.stackCount, 0);
					carryTracker.innerContainer.TransferToContainer(carriedThing, this.Carrier.inventory.innerContainer, carriedThing.stackCount);
				}
			};
		}

		private bool IsUsableCarrier(Pawn p)
		{
			return p == this.pawn || (!p.DestroyedOrNull() && p.Spawned && (p.RaceProps.packAnimal || p.HostFaction == Faction.OfPlayer) && !p.IsBurning() && !p.Downed && !MassUtility.IsOverEncumbered(p));
		}

		private float GetCarrierScore(Pawn p)
		{
			float lengthHorizontal = (p.Position - this.pawn.Position).LengthHorizontal;
			float num = MassUtility.EncumbrancePercent(p);
			float num2 = 1f - num;
			return num2 - lengthHorizontal / 10f * 0.2f;
		}

		private Pawn FindBestCarrier(bool onlyAnimals)
		{
			Lord lord = this.pawn.GetLord();
			Pawn pawn = null;
			float num = 0f;
			if (lord != null)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn2 = lord.ownedPawns[i];
					if (pawn2 != this.pawn)
					{
						if (!onlyAnimals || pawn2.RaceProps.Animal)
						{
							if (this.IsUsableCarrier(pawn2))
							{
								float carrierScore = this.GetCarrierScore(pawn2);
								if (pawn == null || carrierScore > num)
								{
									pawn = pawn2;
									num = carrierScore;
								}
							}
						}
					}
				}
			}
			return pawn;
		}
	}
}
