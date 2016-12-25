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
			if (base.CurJob.def == JobDefOf.Rescue)
			{
				this.FailOnNotDowned(TargetIndex.A);
			}
			this.FailOn(delegate
			{
				if (this.<>f__this.CurJob.def.makeTargetPrisoner)
				{
					if (!this.<>f__this.DropBed.ForPrisoners)
					{
						return true;
					}
				}
				else if (this.<>f__this.DropBed.ForPrisoners != ((Pawn)((Thing)this.<>f__this.TargetA)).IsPrisoner)
				{
					return true;
				}
				return false;
			});
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Reserve.Reserve(TargetIndex.B, this.DropBed.SleepingSlotsCount);
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
			this.globalFinishActions.Add(delegate
			{
				if (this.<>f__this.CurJob.def.makeTargetPrisoner && this.<>f__this.Takee.ownership.OwnedBed == this.<>f__this.DropBed && this.<>f__this.Takee.Position != RestUtility.GetBedSleepingSlotPosFor(this.<>f__this.Takee, this.<>f__this.DropBed))
				{
					this.<>f__this.Takee.ownership.UnclaimBed();
				}
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnNonMedicalBedNotOwned(TargetIndex.B, TargetIndex.A).FailOn(() => this.<>f__this.CurJob.def == JobDefOf.Arrest && !this.<>f__this.Takee.CanBeArrested()).FailOn(() => !this.<>f__this.pawn.CanReach(this.<>f__this.DropBed, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
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
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.CurJob.def.makeTargetPrisoner)
					{
						if (this.<>f__this.Takee.guest.released)
						{
							this.<>f__this.Takee.guest.released = false;
							this.<>f__this.Takee.guest.interactionMode = PrisonerInteractionMode.NoInteraction;
						}
						if (!this.<>f__this.Takee.IsPrisonerOfColony)
						{
							if (this.<>f__this.Takee.Faction != null)
							{
								this.<>f__this.Takee.Faction.Notify_MemberCaptured(this.<>f__this.Takee, this.<>f__this.pawn.Faction);
							}
							this.<>f__this.Takee.guest.SetGuestStatus(Faction.OfPlayer, true);
							if (this.<>f__this.Takee.guest.IsPrisoner)
							{
								TaleRecorder.RecordTale(TaleDefOf.Captured, new object[]
								{
									this.<>f__this.pawn,
									this.<>f__this.Takee
								});
								this.<>f__this.pawn.records.Increment(RecordDefOf.PeopleCaptured);
							}
						}
					}
					else if (this.<>f__this.Takee.Faction != Faction.OfPlayer && this.<>f__this.Takee.HostFaction != Faction.OfPlayer && this.<>f__this.Takee.guest != null)
					{
						this.<>f__this.Takee.guest.SetGuestStatus(Faction.OfPlayer, false);
					}
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
	}
}
