using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AITrashBuildingsDistant : ThinkNode_JobGiver
	{
		public bool attackAllInert;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AITrashBuildingsDistant jobGiver_AITrashBuildingsDistant = (JobGiver_AITrashBuildingsDistant)base.DeepCopy(resolve);
			jobGiver_AITrashBuildingsDistant.attackAllInert = this.attackAllInert;
			return jobGiver_AITrashBuildingsDistant;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			if (allBuildingsColonist.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < 75; i++)
			{
				Building building = allBuildingsColonist.RandomElement<Building>();
				if (TrashUtility.ShouldTrashBuilding(pawn, building, this.attackAllInert))
				{
					return TrashUtility.TrashJob(pawn, building);
				}
			}
			return null;
		}
	}
}
