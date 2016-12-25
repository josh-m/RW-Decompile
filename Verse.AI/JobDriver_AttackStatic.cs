using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_AttackStatic : JobDriver
	{
		private bool startedIncapacitated;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<bool>(ref this.startedIncapacitated, "startedIncapacitated", false, false);
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
					this.<>f__this.pawn.equipment.TryStartAttack(this.<>f__this.TargetA);
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}
	}
}
