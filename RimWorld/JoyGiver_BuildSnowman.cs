using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_BuildSnowman : JoyGiver
	{
		private const float MinSnowmanDepth = 0.5f;

		public override Job TryGiveJob(Pawn pawn)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn, null))
			{
				return null;
			}
			if (pawn.Map.snowGrid.TotalDepth < 200f)
			{
				return null;
			}
			IntVec3 c = JoyGiver_BuildSnowman.TryFindSnowmanBuildCell(pawn);
			if (!c.IsValid)
			{
				return null;
			}
			return new Job(this.def.jobDef, c);
		}

		private static IntVec3 TryFindSnowmanBuildCell(Pawn pawn)
		{
			Region rootReg;
			if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), (Region r) => r.Room.PsychologicallyOutdoors, 100, out rootReg))
			{
				return IntVec3.Invalid;
			}
			IntVec3 result = IntVec3.Invalid;
			RegionTraverser.BreadthFirstTraverse(rootReg, (Region from, Region r) => r.Room == rootReg.Room, delegate(Region r)
			{
				for (int i = 0; i < 5; i++)
				{
					IntVec3 randomCell = r.RandomCell;
					if (JoyGiver_BuildSnowman.IsGoodSnowmanCell(randomCell, pawn))
					{
						result = randomCell;
						return true;
					}
				}
				return false;
			}, 30);
			return result;
		}

		private static bool IsGoodSnowmanCell(IntVec3 c, Pawn pawn)
		{
			if (pawn.Map.snowGrid.GetDepth(c) < 0.5f)
			{
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				return false;
			}
			if (c.GetEdifice(pawn.Map) != null)
			{
				return false;
			}
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
				if (!c2.InBounds(pawn.Map))
				{
					return false;
				}
				if (!c2.Standable(pawn.Map))
				{
					return false;
				}
				if (pawn.Map.reservationManager.IsReserved(c2, pawn.Faction))
				{
					return false;
				}
			}
			return true;
		}
	}
}
