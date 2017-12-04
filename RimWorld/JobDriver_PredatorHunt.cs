using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PredatorHunt : JobDriver
	{
		private bool notifiedPlayer;

		private bool firstHit = true;

		public const TargetIndex PreyInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const int MaxHuntTicks = 5000;

		public Pawn Prey
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
			Scribe_Values.Look<bool>(ref this.firstHit, "firstHit", false, false);
		}

		public override string GetReport()
		{
			if (this.Corpse != null)
			{
				return base.ReportStringProcessed(JobDefOf.Ingest.reportString);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			base.AddFinishAction(delegate
			{
				this.$this.Map.attackTargetsCache.UpdateTarget(this.$this.pawn);
			});
			Toil prepareToEatCorpse = new Toil();
			prepareToEatCorpse.initAction = delegate
			{
				Pawn actor = prepareToEatCorpse.actor;
				Corpse corpse = this.$this.Corpse;
				if (corpse == null)
				{
					Pawn prey = this.$this.Prey;
					if (prey == null)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						return;
					}
					corpse = prey.Corpse;
					if (corpse == null || !corpse.Spawned)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						return;
					}
				}
				if (actor.Faction == Faction.OfPlayer)
				{
					corpse.SetForbidden(false, false);
				}
				else
				{
					corpse.SetForbidden(true, false);
				}
				actor.CurJob.SetTarget(TargetIndex.A, corpse);
			};
			yield return Toils_General.DoAtomic(delegate
			{
				this.$this.Map.attackTargetsCache.UpdateTarget(this.$this.pawn);
			});
			Action onHitAction = delegate
			{
				Pawn prey = this.$this.Prey;
				bool surpriseAttack = this.$this.firstHit && !prey.IsColonist;
				if (this.$this.pawn.meleeVerbs.TryMeleeAttack(prey, this.$this.job.verbToUse, surpriseAttack))
				{
					if (!this.$this.notifiedPlayer && PawnUtility.ShouldSendNotificationAbout(prey))
					{
						this.$this.notifiedPlayer = true;
						Messages.Message("MessageAttackedByPredator".Translate(new object[]
						{
							prey.LabelShort,
							this.$this.pawn.LabelIndefinite()
						}).CapitalizeFirst(), prey, MessageTypeDefOf.ThreatSmall);
					}
					this.$this.Map.attackTargetsCache.UpdateTarget(this.$this.pawn);
				}
				this.$this.firstHit = false;
			};
			yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, onHitAction).JumpIfDespawnedOrNull(TargetIndex.A, prepareToEatCorpse).JumpIf(() => this.$this.Corpse != null, prepareToEatCorpse).FailOn(() => Find.TickManager.TicksGame > this.$this.startTick + 5000 && (float)(this.$this.job.GetTarget(TargetIndex.A).Cell - this.$this.pawn.Position).LengthHorizontalSquared > 4f);
			yield return prepareToEatCorpse;
			Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return gotoCorpse;
			float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
			yield return Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, TargetIndex.A, TargetIndex.None).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(gotoCorpse, () => this.$this.pawn.needs.food.CurLevelPercentage < 0.9f);
		}
	}
}
