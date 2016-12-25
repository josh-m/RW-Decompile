using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Manhunter : ThinkNode_JobGiver
	{
		private const float WaitChance = 0.75f;

		private const int WaitTicks = 90;

		private const int MinMeleeChaseTicks = 420;

		private const int MaxMeleeChaseTicks = 900;

		private const int WanderOutsideDoorRegions = 9;

		protected override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = this.FindPawnTarget(pawn);
			if (thing != null && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				return this.MeleeAttackJob(pawn, thing);
			}
			Thing thing2 = this.FindTurretTarget(pawn);
			if (thing2 != null)
			{
				return this.MeleeAttackJob(pawn, thing2);
			}
			if (thing != null)
			{
				using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, thing.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
				{
					Job result;
					if (!pawnPath.Found)
					{
						result = null;
						return result;
					}
					IntVec3 loc;
					if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out loc))
					{
						Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + thing.LabelCap);
						result = null;
						return result;
					}
					IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null).RandomCell;
					Job job = new Job(JobDefOf.Goto, randomCell);
					result = job;
					return result;
				}
			}
			return null;
		}

		private Job MeleeAttackJob(Pawn pawn, Thing target)
		{
			return new Job(JobDefOf.AttackMelee, target)
			{
				maxNumMeleeAttacks = 1,
				expiryInterval = Rand.Range(420, 900),
				attackDoorIfTargetLost = true
			};
		}

		private Thing FindPawnTarget(Pawn pawn)
		{
			return AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
		}

		private Thing FindTurretTarget(Pawn pawn)
		{
			return AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default(IntVec3), 3.40282347E+38f, false);
		}
	}
}
