using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Berserk : ThinkNode_JobGiver
	{
		private const float MaxAttackDistance = 30f;

		private const float WaitChance = 0.5f;

		private const int WaitTicks = 90;

		private const int MinMeleeChaseTicks = 420;

		private const int MaxMeleeChaseTicks = 900;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Rand.Value < 0.5f)
			{
				return new Job(JobDefOf.WaitCombat)
				{
					expiryInterval = 90
				};
			}
			if (pawn.TryGetAttackVerb(false) == null)
			{
				return null;
			}
			Pawn pawn2 = this.FindPawnTarget(pawn);
			if (pawn2 != null)
			{
				return new Job(JobDefOf.AttackMelee, pawn2)
				{
					maxNumMeleeAttacks = 1,
					expiryInterval = Rand.Range(420, 900),
					canBash = true
				};
			}
			return null;
		}

		private Pawn FindPawnTarget(Pawn pawn)
		{
			return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing x) => x is Pawn, 0f, 30f, default(IntVec3), 3.40282347E+38f, true);
		}
	}
}
