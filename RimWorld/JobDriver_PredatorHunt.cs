using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PredatorHunt : JobDriver
	{
		public const TargetIndex PreyInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const int MaxHuntTicks = 5000;

		private bool notifiedPlayer;

		private bool firstHit = true;

		public Pawn Prey
		{
			get
			{
				Corpse corpse = this.Corpse;
				if (corpse != null)
				{
					return corpse.innerPawn;
				}
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Corpse Corpse
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing as Corpse;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<bool>(ref this.firstHit, "firstHit", false, false);
		}

		public override string GetReport()
		{
			if (this.Corpse != null)
			{
				return base.ReportStringProcessed(JobDefOf.Ingest.reportString);
			}
			return base.GetReport();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			base.AddFinishAction(delegate
			{
				Find.AttackTargetsCache.UpdateTarget(this.<>f__this.pawn);
			});
			Toil prepareToEatCorpse = new Toil();
			prepareToEatCorpse.initAction = delegate
			{
				Pawn actor = this.<prepareToEatCorpse>__0.actor;
				Pawn prey = this.<>f__this.Prey;
				if (prey == null)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					return;
				}
				Corpse corpse = HuntJobUtility.TryFindCorpse(prey);
				if (corpse == null)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					return;
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
			yield return new Toil
			{
				initAction = delegate
				{
					Find.AttackTargetsCache.UpdateTarget(this.<>f__this.pawn);
				},
				atomicWithPrevious = true,
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			Action onHitAction = delegate
			{
				Pawn prey = this.<>f__this.Prey;
				bool surpriseAttack = this.<>f__this.firstHit && !prey.IsColonist;
				if (this.<>f__this.pawn.meleeVerbs.TryMeleeAttack(prey, this.<>f__this.CurJob.verbToUse, surpriseAttack))
				{
					if (!this.<>f__this.notifiedPlayer && PawnUtility.ShouldSendNotificationAbout(prey))
					{
						this.<>f__this.notifiedPlayer = true;
						Messages.Message("MessageAttackedByPredator".Translate(new object[]
						{
							prey.LabelShort,
							this.<>f__this.pawn.LabelIndefinite()
						}).CapitalizeFirst(), prey, MessageSound.SeriousAlert);
					}
					Find.AttackTargetsCache.UpdateTarget(this.<>f__this.pawn);
				}
				this.<>f__this.firstHit = false;
			};
			yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, onHitAction).JumpIfDespawnedOrNull(TargetIndex.A, prepareToEatCorpse).FailOn(() => Find.TickManager.TicksGame > this.<>f__this.startTick + 5000 && (this.<>f__this.CurJob.GetTarget(TargetIndex.A).Cell - this.<>f__this.pawn.Position).LengthHorizontalSquared > 4f);
			yield return prepareToEatCorpse;
			Toil gotoCorpse = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			yield return gotoCorpse;
			float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
			yield return Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, TargetIndex.A, TargetIndex.None).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(gotoCorpse, () => this.<>f__this.pawn.needs.food.CurLevelPercentage < 0.9f);
		}
	}
}
