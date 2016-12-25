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
			if (Find.SnowGrid.TotalDepth < 200f)
			{
				return null;
			}
			IntVec3 vec = JoyGiver_BuildSnowman.TryFindSnowmanBuildCell(pawn);
			if (!vec.IsValid)
			{
				return null;
			}
			return new Job(this.def.jobDef, vec);
		}

		private static IntVec3 TryFindSnowmanBuildCell(Pawn pawn)
		{
			Region rootReg;
			if (!CellFinder.TryFindClosestRegionWith(pawn.Position.GetRegion(), TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), (Region r) => r.Room.PsychologicallyOutdoors, 100, out rootReg))
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
			if (Find.SnowGrid.GetDepth(c) < 0.5f)
			{
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				return false;
			}
			if (c.GetEdifice() != null)
			{
				return false;
			}
			for (int i = 0; i < 9; i++)
			{
				IntVec3 intVec = c + GenAdj.AdjacentCellsAndInside[i];
				if (!intVec.InBounds())
				{
					return false;
				}
				if (!intVec.Standable())
				{
					return false;
				}
				if (Find.Reservations.IsReserved(intVec, pawn.Faction))
				{
					return false;
				}
			}
			return true;
		}
	}
}
