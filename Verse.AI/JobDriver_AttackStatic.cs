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
					else if (this.$this.job.endIfCantShootTargetFromCurPos && !this.$this.pawn.stances.FullBodyBusy)
					{
						Verb verb = this.$this.pawn.TryGetAttackVerb(this.$this.TargetA.Thing, !this.$this.pawn.IsColonist);
						if (verb == null || !verb.CanHitTargetFrom(this.$this.pawn.Position, this.$this.TargetA))
						{
							this.$this.EndJobWith(JobCondition.Incompletable);
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}
	}
}
