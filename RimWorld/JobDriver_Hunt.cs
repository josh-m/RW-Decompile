using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Hunt : JobDriver
	{
		private int jobStartTick = -1;

		private const TargetIndex VictimInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		private const int MaxHuntTicks = 5000;

		public Pawn Victim
		{
			get
			{
				Corpse corpse = this.Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Corpse Corpse
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing as Corpse;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.jobStartTick, "jobStartTick", 0, false);
		}

		public override string GetReport()
		{
			if (this.Victim != null)
			{
				return this.job.def.reportString.Replace("TargetA", this.Victim.LabelShort);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Victim, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate
			{
				if (!this.$this.job.ignoreDesignations)
				{
					Pawn victim = this.$this.Victim;
					if (victim != null && !victim.Dead && this.$this.Map.designationManager.DesignationOn(victim, DesignationDefOf.Hunt) == null)
					{
						return true;
					}
				}
				return false;
			});
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.jobStartTick = Find.TickManager.TicksGame;
				}
			};
			yield return Toils_Combat.TrySetJobToUseAttackVerb();
			Toil startCollectCorpse = this.StartCollectCorpseToil();
			Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, true).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > this.$this.jobStartTick + 5000);
			yield return gotoCastPos;
			Toil moveIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
			yield return moveIfCannotHit;
			yield return Toils_Jump.JumpIfTargetDownedDistant(TargetIndex.A, gotoCastPos);
			yield return Toils_Combat.CastVerb(TargetIndex.A, false).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > this.$this.jobStartTick + 5000);
			yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpse);
			yield return Toils_Jump.Jump(moveIfCannotHit);
			yield return startCollectCorpse;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
		}

		private Toil StartCollectCorpseToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (this.Victim == null)
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.Hunted, new object[]
				{
					this.pawn,
					this.Victim
				});
				Corpse corpse = this.Victim.Corpse;
				if (corpse == null || !this.pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false))
				{
					this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				corpse.SetForbidden(false, true);
				IntVec3 c;
				if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, this.pawn, this.Map, StoragePriority.Unstored, this.pawn.Faction, out c, true))
				{
					this.pawn.Reserve(corpse, this.job, 1, -1, null);
					this.pawn.Reserve(c, this.job, 1, -1, null);
					this.job.SetTarget(TargetIndex.B, c);
					this.job.SetTarget(TargetIndex.A, corpse);
					this.job.count = 1;
					this.job.haulMode = HaulMode.ToCellStorage;
					return;
				}
				this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
			};
			return toil;
		}
	}
}
