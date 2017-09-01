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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn pawn = this.<>f__this.TargetThingA as Pawn;
					if (pawn != null)
					{
						this.<>f__this.startedIncapacitated = pawn.Downed;
					}
					this.<>f__this.pawn.pather.StopDead();
				},
				tickAction = delegate
				{
					if (this.<>f__this.TargetA.HasThing)
					{
						Pawn pawn = this.<>f__this.TargetA.Thing as Pawn;
						if (this.<>f__this.TargetA.Thing.Destroyed || (pawn != null && !this.<>f__this.startedIncapacitated && pawn.Downed))
						{
							this.<>f__this.EndJobWith(JobCondition.Succeeded);
							return;
						}
					}
					if (this.<>f__this.numAttacksMade >= this.<>f__this.pawn.CurJob.maxNumStaticAttacks && !this.<>f__this.pawn.stances.FullBodyBusy)
					{
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
						return;
					}
					if (this.<>f__this.pawn.equipment.TryStartAttack(this.<>f__this.TargetA))
					{
						this.<>f__this.numAttacksMade++;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}
	}
}
