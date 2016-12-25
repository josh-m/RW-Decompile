using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class DigUtility
	{
		private const int CheckOverrideInterval = 500;

		public static Job PassBlockerJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker, bool canMineNonMineables)
		{
			if (blocker.def.mineable)
			{
				return DigUtility.MineOrWaitJob(pawn, blocker, cellBeforeBlocker);
			}
			if (pawn.equipment != null && pawn.equipment.Primary != null)
			{
				Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
				if (primaryVerb.verbProps.ai_IsBuildingDestroyer && (!primaryVerb.verbProps.ai_IsIncendiary || blocker.FlammableNow))
				{
					return new Job(JobDefOf.UseVerbOnThing)
					{
						targetA = blocker,
						verbToUse = primaryVerb,
						expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange
					};
				}
			}
			if (canMineNonMineables)
			{
				return DigUtility.MineOrWaitJob(pawn, blocker, cellBeforeBlocker);
			}
			return DigUtility.MeleeOrWaitJob(pawn, blocker, cellBeforeBlocker);
		}

		private static Job MeleeOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			if (!pawn.CanReserve(blocker, 1))
			{
				return new Job(JobDefOf.Goto, CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10), 500, true);
			}
			return new Job(JobDefOf.AttackMelee, blocker)
			{
				ignoreDesignations = true,
				expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
				checkOverrideOnExpire = true
			};
		}

		private static Job MineOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			if (!pawn.CanReserve(blocker, 1))
			{
				return new Job(JobDefOf.Goto, CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10), 500, true);
			}
			return new Job(JobDefOf.Mine, blocker)
			{
				ignoreDesignations = true,
				expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
				checkOverrideOnExpire = true
			};
		}
	}
}
