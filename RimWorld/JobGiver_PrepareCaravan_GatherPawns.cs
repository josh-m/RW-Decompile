using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_PrepareCaravan_GatherPawns : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = this.FindPawn(pawn);
			if (pawn2 == null)
			{
				return null;
			}
			return new Job(JobDefOf.PrepareCaravan_GatherPawns, pawn2);
		}

		private Pawn FindPawn(Pawn pawn)
		{
			if (pawn.mindState.duty.pawnsToGather == PawnsToGather.None)
			{
				return null;
			}
			float num = 0f;
			Pawn pawn2 = null;
			Lord lord = pawn.GetLord();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn3 = lord.ownedPawns[i];
				if (pawn3 != pawn)
				{
					if (!pawn3.IsColonist)
					{
						if (pawn.mindState.duty.pawnsToGather != PawnsToGather.Slaves || !pawn3.RaceProps.Animal)
						{
							if (pawn.mindState.duty.pawnsToGather != PawnsToGather.Animals || pawn3.RaceProps.Animal)
							{
								if (!GatherAnimalsAndSlavesForCaravanUtility.IsFollowingAnyone(pawn3))
								{
									float num2 = pawn.Position.DistanceToSquared(pawn3.Position);
									if (pawn2 == null || num2 < num)
									{
										if (pawn.CanReserveAndReach(pawn3, PathEndMode.Touch, Danger.Deadly, 1))
										{
											pawn2 = pawn3;
											num = num2;
										}
									}
								}
							}
						}
					}
				}
			}
			return pawn2;
		}
	}
}
