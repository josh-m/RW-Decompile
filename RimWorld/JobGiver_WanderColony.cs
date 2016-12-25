using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_WanderColony : JobGiver_Wander
	{
		private static List<IntVec3> gatherSpots = new List<IntVec3>();

		public JobGiver_WanderColony()
		{
			this.wanderRadius = 7f;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
			this.wanderDestValidator = ((Pawn pawn, IntVec3 loc) => true);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike)
			{
				JobGiver_WanderColony.gatherSpots.Clear();
				for (int i = 0; i < pawn.Map.gatherSpotLister.activeSpots.Count; i++)
				{
					IntVec3 position = pawn.Map.gatherSpotLister.activeSpots[i].parent.Position;
					if (!position.IsForbidden(pawn) && pawn.CanReach(position, PathEndMode.Touch, Danger.None, false, TraverseMode.ByPawn))
					{
						JobGiver_WanderColony.gatherSpots.Add(position);
					}
				}
				if (JobGiver_WanderColony.gatherSpots.Count > 0)
				{
					return JobGiver_WanderColony.gatherSpots.RandomElement<IntVec3>();
				}
			}
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			if (allBuildingsColonist.Count != 0)
			{
				int num = 0;
				while (true)
				{
					num++;
					if (num > 20)
					{
						break;
					}
					Building building = allBuildingsColonist.RandomElement<Building>();
					if (!building.Position.IsForbidden(pawn) && pawn.Map.areaManager.Home[building.Position])
					{
						int num2 = 15 + num * 2;
						if ((pawn.Position - building.Position).LengthHorizontalSquared <= (float)(num2 * num2))
						{
							IntVec3 intVec = GenAdj.CellsAdjacent8Way(building).ToList<IntVec3>().RandomElement<IntVec3>();
							if (intVec.Standable(building.Map) && (num >= 12 || !intVec.IsForbidden(pawn)) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.None, false, TraverseMode.ByPawn) && !intVec.IsInPrisonCell(pawn.Map))
							{
								return intVec;
							}
						}
					}
				}
				return pawn.Position;
			}
			Pawn pawn2;
			if ((from c in pawn.Map.mapPawns.FreeColonistsSpawned
			where !c.Position.IsForbidden(pawn)
			select c).TryRandomElement(out pawn2))
			{
				return pawn2.Position;
			}
			return pawn.Position;
		}
	}
}
