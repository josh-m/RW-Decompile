using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_AttackMelee : JobDriver
	{
		private int numMeleeAttacksMade;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.numMeleeAttacksMade, "numMeleeAttacksMade", 0, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_ReserveAttackTarget.TryReserve(TargetIndex.A);
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
			{
				Thing thing = this.<>f__this.CurJob.GetTarget(TargetIndex.A).Thing;
				if (this.<>f__this.pawn.meleeVerbs.TryMeleeAttack(thing, this.<>f__this.CurJob.verbToUse, false))
				{
					if (this.<>f__this.pawn.CurJob == null || this.<>f__this.pawn.jobs.curDriver != this.<>f__this)
					{
						return;
					}
					this.<>f__this.numMeleeAttacksMade++;
					if (this.<>f__this.numMeleeAttacksMade >= this.<>f__this.pawn.CurJob.maxNumMeleeAttacks)
					{
						this.<>f__this.EndJobWith(JobCondition.Succeeded);
						return;
					}
				}
			}).FailOnDespawnedOrNull(TargetIndex.A);
		}

		public override void Notify_PatherFailed()
		{
			if (base.CurJob.attackDoorIfTargetLost)
			{
				Thing thing;
				using (PawnPath pawnPath = PathFinder.FindPath(this.pawn.Position, base.TargetA.Cell, TraverseParms.For(this.pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
				{
					if (!pawnPath.Found)
					{
						return;
					}
					IntVec3 intVec;
					thing = pawnPath.FirstBlockingBuilding(out intVec, this.pawn);
				}
				if (thing != null)
				{
					base.CurJob.targetA = thing;
					base.CurJob.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
					base.CurJob.expiryInterval = Rand.Range(2000, 4000);
					return;
				}
			}
			base.Notify_PatherFailed();
		}

		public override bool IsContinuation(Job j)
		{
			return base.CurJob.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}
	}
}
