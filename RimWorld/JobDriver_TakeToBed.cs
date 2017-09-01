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
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Building_Bed DropBed
		{
			get
			{
				return (Building_Bed)base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(delegate
			{
				if (this.<>f__this.CurJob.def.makeTargetPrisoner)
				{
					if (!this.<>f__this.DropBed.ForPrisoners)
					{
						return true;
					}
				}
				else if (this.<>f__this.DropBed.ForPrisoners != this.<>f__this.Takee.IsPrisoner)
				{
					return true;
				}
				return false;
			});
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Reserve.Reserve(TargetIndex.B, this.DropBed.SleepingSlotsCount, 0, null);
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
			base.AddFinishAction(delegate
			{
				if (this.<>f__this.CurJob.def.makeTargetPrisoner && this.<>f__this.Takee.ownership.OwnedBed == this.<>f__this.DropBed && this.<>f__this.Takee.Position != RestUtility.GetBedSleepingSlotPosFor(this.<>f__this.Takee, this.<>f__this.DropBed))
				{
					this.<>f__this.Takee.ownership.UnclaimBed();
				}
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnNonMedicalBedNotOwned(TargetIndex.B, TargetIndex.A).FailOn(() => this.<>f__this.CurJob.def == JobDefOf.Arrest && !this.<>f__this.Takee.CanBeArrested()).FailOn(() => !this.<>f__this.pawn.CanReach(this.<>f__this.DropBed, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOn(() => this.<>f__this.CurJob.def == JobDefOf.Rescue && !this.<>f__this.Takee.Downed).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.CurJob.def.makeTargetPrisoner)
					{
						Pawn pawn = (Pawn)this.<>f__this.CurJob.targetA.Thing;
						Lord lord = pawn.GetLord();
						if (lord != null)
						{
							lord.Notify_PawnAttemptArrested(pawn);
						}
						GenClamor.DoClamor(pawn, 10f, ClamorType.Harm);
						if (this.<>f__this.CurJob.def == JobDefOf.Arrest && !pawn.CheckAcceptArrest(this.<>f__this.pawn))
						{
							this.<>f__this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						}
					}
				}
			};
			Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			startCarrying.AddPreInitAction(new Action(this.CheckMakeTakeeGuest));
			yield return startCarrying;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.CheckMakeTakeePrisoner();
					if (this.<>f__this.Takee.playerSettings == null)
					{
						this.<>f__this.Takee.playerSettings = new Pawn_PlayerSettings(this.<>f__this.Takee);
					}
				}
			};
			yield return Toils_Reserve.Release(TargetIndex.B);
			yield return new Toil
			{
				initAction = delegate
				{
					IntVec3 position = this.<>f__this.DropBed.Position;
					Thing thing;
					this.<>f__this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing, null);
					if (!this.<>f__this.DropBed.Destroyed && (this.<>f__this.DropBed.owners.Contains(this.<>f__this.Takee) || (this.<>f__this.DropBed.Medical && this.<>f__this.DropBed.AnyUnoccupiedSleepingSlot) || this.<>f__this.Takee.ownership == null))
					{
						this.<>f__this.Takee.jobs.Notify_TuckedIntoBed(this.<>f__this.DropBed);
						if (this.<>f__this.Takee.RaceProps.Humanlike && this.<>f__this.CurJob.def != JobDefOf.Arrest && !this.<>f__this.Takee.IsPrisonerOfColony)
						{
							this.<>f__this.Takee.relations.Notify_RescuedBy(this.<>f__this.pawn);
						}
					}
					if (this.<>f__this.Takee.IsPrisonerOfColony)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, this.<>f__this.Takee, OpportunityType.GoodToKnow);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private void CheckMakeTakeePrisoner()
		{
			if (base.CurJob.def.makeTargetPrisoner)
			{
				if (this.Takee.guest.released)
				{
					this.Takee.guest.released = false;
					this.Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
				}
				if (!this.Takee.IsPrisonerOfColony)
				{
					if (this.Takee.Faction != null)
					{
						this.Takee.Faction.Notify_MemberCaptured(this.Takee, this.pawn.Faction);
					}
					this.Takee.guest.SetGuestStatus(Faction.OfPlayer, true);
					if (this.Takee.guest.IsPrisoner)
					{
						TaleRecorder.RecordTale(TaleDefOf.Captured, new object[]
						{
							this.pawn,
							this.Takee
						});
						this.pawn.records.Increment(RecordDefOf.PeopleCaptured);
					}
				}
			}
		}

		private void CheckMakeTakeeGuest()
		{
			if (!base.CurJob.def.makeTargetPrisoner && this.Takee.Faction != Faction.OfPlayer && this.Takee.HostFaction != Faction.OfPlayer && this.Takee.guest != null)
			{
				this.Takee.guest.SetGuestStatus(Faction.OfPlayer, false);
			}
		}
	}
}
