using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PredatorHunt : JobDriver
	{
		private bool notifiedPlayerAttacked;

		private bool notifiedPlayerAttacking;

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
			Scribe_Values.Look<bool>(ref this.notifiedPlayerAttacking, "notifiedPlayerAttacking", false, false);
		}

		public override string GetReport()
		{
			if (this.Corpse != null)
			{
				return base.ReportStringProcessed(JobDefOf.Ingest.reportString);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
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
					if (!this.$this.notifiedPlayerAttacked && PawnUtility.ShouldSendNotificationAbout(prey))
					{
						this.$this.notifiedPlayerAttacked = true;
						Messages.Message("MessageAttackedByPredator".Translate(prey.LabelShort, this.$this.pawn.LabelIndefinite(), prey.Named("PREY"), this.$this.pawn.Named("PREDATOR")).CapitalizeFirst(), prey, MessageTypeDefOf.ThreatSmall, true);
					}
					this.$this.Map.attackTargetsCache.UpdateTarget(this.$this.pawn);
					this.$this.firstHit = false;
				}
			};
			Toil followAndAttack = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, onHitAction).JumpIfDespawnedOrNull(TargetIndex.A, prepareToEatCorpse).JumpIf(() => this.$this.Corpse != null, prepareToEatCorpse).FailOn(() => Find.TickManager.TicksGame > this.$this.startTick + 5000 && (float)(this.$this.job.GetTarget(TargetIndex.A).Cell - this.$this.pawn.Position).LengthHorizontalSquared > 4f);
			followAndAttack.AddPreTickAction(new Action(this.CheckWarnPlayer));
			yield return followAndAttack;
			yield return prepareToEatCorpse;
			Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return gotoCorpse;
			float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
			yield return Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, TargetIndex.A, TargetIndex.None).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(gotoCorpse, () => this.$this.pawn.needs.food.CurLevelPercentage < 0.9f);
		}

		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(this.pawn) && dinfo.Def.isRanged && dinfo.Instigator != null && dinfo.Instigator != this.Prey && !this.pawn.InMentalState && !this.pawn.Downed)
			{
				this.pawn.mindState.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
			}
		}

		private void CheckWarnPlayer()
		{
			if (this.notifiedPlayerAttacking)
			{
				return;
			}
			Pawn prey = this.Prey;
			if (!prey.Spawned || prey.Faction != Faction.OfPlayer)
			{
				return;
			}
			if (Find.TickManager.TicksGame <= this.pawn.mindState.lastPredatorHuntingPlayerNotificationTick + 2500)
			{
				return;
			}
			if (!prey.Position.InHorDistOf(this.pawn.Position, 60f))
			{
				return;
			}
			if (prey.RaceProps.Humanlike)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelPredatorHuntingColonist".Translate(this.pawn.LabelShort, prey.LabelDefinite(), this.pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), "LetterPredatorHuntingColonist".Translate(this.pawn.LabelIndefinite(), prey.LabelDefinite(), this.pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), LetterDefOf.ThreatBig, this.pawn, null, null);
			}
			else
			{
				Messages.Message("MessagePredatorHuntingPlayerAnimal".Translate(this.pawn.LabelIndefinite(), prey.LabelDefinite(), this.pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), this.pawn, MessageTypeDefOf.ThreatBig, true);
			}
			this.pawn.mindState.Notify_PredatorHuntingPlayerNotification();
			this.notifiedPlayerAttacking = true;
		}
	}
}
