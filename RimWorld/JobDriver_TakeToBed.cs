using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_TakeToBed : JobDriver
	{
		private const TargetIndex TakeeIndex = TargetIndex.A;

		private const TargetIndex BedIndex = TargetIndex.B;

		protected Pawn Takee
		{
			get
			{
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Building_Bed DropBed
		{
			get
			{
				return (Building_Bed)this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.Takee;
			Job job = this.job;
			bool arg_6A_0;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				pawn = this.pawn;
				target = this.DropBed;
				job = this.job;
				int sleepingSlotsCount = this.DropBed.SleepingSlotsCount;
				int stackCount = 0;
				arg_6A_0 = pawn.Reserve(target, job, sleepingSlotsCount, stackCount, null, errorOnFailed);
			}
			else
			{
				arg_6A_0 = false;
			}
			return arg_6A_0;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
			this.FailOn(delegate
			{
				if (this.$this.job.def.makeTargetPrisoner)
				{
					if (!this.$this.DropBed.ForPrisoners)
					{
						return true;
					}
				}
				else if (this.$this.DropBed.ForPrisoners != this.$this.Takee.IsPrisoner)
				{
					return true;
				}
				return false;
			});
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
			base.AddFinishAction(delegate
			{
				if (this.$this.job.def.makeTargetPrisoner && this.$this.Takee.ownership.OwnedBed == this.$this.DropBed && this.$this.Takee.Position != RestUtility.GetBedSleepingSlotPosFor(this.$this.Takee, this.$this.DropBed))
				{
					this.$this.Takee.ownership.UnclaimBed();
				}
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.$this.job.def == JobDefOf.Arrest && !this.$this.Takee.CanBeArrestedBy(this.$this.pawn)).FailOn(() => !this.$this.pawn.CanReach(this.$this.DropBed, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOn(() => this.$this.job.def == JobDefOf.Rescue && !this.$this.Takee.Downed).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.$this.job.def.makeTargetPrisoner)
					{
						Pawn pawn = (Pawn)this.$this.job.targetA.Thing;
						Lord lord = pawn.GetLord();
						if (lord != null)
						{
							lord.Notify_PawnAttemptArrested(pawn);
						}
						GenClamor.DoClamor(pawn, 10f, ClamorDefOf.Harm);
						if (this.$this.job.def == JobDefOf.Arrest && !pawn.CheckAcceptArrest(this.$this.pawn))
						{
							this.$this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						}
					}
				}
			};
			Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false).FailOnNonMedicalBedNotOwned(TargetIndex.B, TargetIndex.A);
			startCarrying.AddPreInitAction(new Action(this.CheckMakeTakeeGuest));
			yield return startCarrying;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.CheckMakeTakeePrisoner();
					if (this.$this.Takee.playerSettings == null)
					{
						this.$this.Takee.playerSettings = new Pawn_PlayerSettings(this.$this.Takee);
					}
				}
			};
			yield return Toils_Reserve.Release(TargetIndex.B);
			yield return new Toil
			{
				initAction = delegate
				{
					IntVec3 position = this.$this.DropBed.Position;
					Thing thing;
					this.$this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing, null);
					if (!this.$this.DropBed.Destroyed && (this.$this.DropBed.owners.Contains(this.$this.Takee) || (this.$this.DropBed.Medical && this.$this.DropBed.AnyUnoccupiedSleepingSlot) || this.$this.Takee.ownership == null))
					{
						this.$this.Takee.jobs.Notify_TuckedIntoBed(this.$this.DropBed);
						if (this.$this.Takee.RaceProps.Humanlike && this.$this.job.def != JobDefOf.Arrest && !this.$this.Takee.IsPrisonerOfColony)
						{
							this.$this.Takee.relations.Notify_RescuedBy(this.$this.pawn);
						}
						this.$this.Takee.mindState.Notify_TuckedIntoBed();
					}
					if (this.$this.Takee.IsPrisonerOfColony)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, this.$this.Takee, OpportunityType.GoodToKnow);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private void CheckMakeTakeePrisoner()
		{
			if (this.job.def.makeTargetPrisoner)
			{
				if (this.Takee.guest.Released)
				{
					this.Takee.guest.Released = false;
					this.Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
				}
				if (!this.Takee.IsPrisonerOfColony)
				{
					this.Takee.guest.CapturedBy(Faction.OfPlayer, this.pawn);
				}
			}
		}

		private void CheckMakeTakeeGuest()
		{
			if (!this.job.def.makeTargetPrisoner && this.Takee.Faction != Faction.OfPlayer && this.Takee.HostFaction != Faction.OfPlayer && this.Takee.guest != null && !this.Takee.IsWildMan())
			{
				this.Takee.guest.SetGuestStatus(Faction.OfPlayer, false);
			}
		}
	}
}
