using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_AttackStatic : JobDriver
	{
		private bool startedIncapacitated;

		private int numAttacksMade;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.startedIncapacitated, "startedIncapacitated", false, false);
			Scribe_Values.Look<int>(ref this.numAttacksMade, "numAttacksMade", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn pawn = this.$this.TargetThingA as Pawn;
					if (pawn != null)
					{
						this.$this.startedIncapacitated = pawn.Downed;
					}
					this.$this.pawn.pather.StopDead();
				},
				tickAction = delegate
				{
					if (!this.$this.TargetA.IsValid)
					{
						this.$this.EndJobWith(JobCondition.Succeeded);
						return;
					}
					if (this.$this.TargetA.HasThing)
					{
						Pawn pawn = this.$this.TargetA.Thing as Pawn;
						if (this.$this.TargetA.Thing.Destroyed || (pawn != null && !this.$this.startedIncapacitated && pawn.Downed))
						{
							this.$this.EndJobWith(JobCondition.Succeeded);
							return;
						}
					}
					if (this.$this.numAttacksMade >= this.$this.job.maxNumStaticAttacks && !this.$this.pawn.stances.FullBodyBusy)
					{
						this.$this.EndJobWith(JobCondition.Succeeded);
						return;
					}
					if (this.$this.pawn.TryStartAttack(this.$this.TargetA))
					{
						this.$this.numAttacksMade++;
					}
					else if (!this.$this.pawn.stances.FullBodyBusy)
					{
						Verb verb = this.$this.pawn.TryGetAttackVerb(this.$this.TargetA.Thing, !this.$this.pawn.IsColonist);
						if (this.$this.job.endIfCantShootTargetFromCurPos && (verb == null || !verb.CanHitTargetFrom(this.$this.pawn.Position, this.$this.TargetA)))
						{
							this.$this.EndJobWith(JobCondition.Incompletable);
							return;
						}
						if (this.$this.job.endIfCantShootInMelee)
						{
							if (verb == null)
							{
								this.$this.EndJobWith(JobCondition.Incompletable);
								return;
							}
							float num = verb.verbProps.EffectiveMinRange(this.$this.TargetA, this.$this.pawn);
							if ((float)this.$this.pawn.Position.DistanceToSquared(this.$this.TargetA.Cell) < num * num && this.$this.pawn.Position.AdjacentTo8WayOrInside(this.$this.TargetA.Cell))
							{
								this.$this.EndJobWith(JobCondition.Incompletable);
								return;
							}
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}
	}
}
