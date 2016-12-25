using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AITrashBuildingsDistant : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			int count = allBuildingsColonist.Count;
			if (count == 0)
			{
				return null;
			}
			for (int i = 0; i < 75; i++)
			{
				int index = Rand.Range(0, count);
				Building building = allBuildingsColonist[index];
				if (TrashUtility.ShouldTrashBuilding(pawn, building))
				{
					return TrashUtility.TrashJob(pawn, building);
				}
			}
			return null;
		}
	}
}
