using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SeekSafeTemperature : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.health.hediffSet.HasTemperatureInjury(TemperatureInjuryStage.Serious))
			{
				return null;
			}
			FloatRange tempRange = pawn.ComfortableTemperatureRange();
			if (tempRange.Includes(pawn.AmbientTemperature))
			{
				return new Job(JobDefOf.WaitSafeTemperature, 500, true);
			}
			Region region = JobGiver_SeekSafeTemperature.ClosestRegionWithinTemperatureRange(pawn.Position, pawn.Map, tempRange, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), RegionType.Set_Passable);
			if (region != null)
			{
				return new Job(JobDefOf.GotoSafeTemperature, region.RandomCell);
			}
			return null;
		}

		private static Region ClosestRegionWithinTemperatureRange(IntVec3 root, Map map, FloatRange tempRange, TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
		{
			Region region = root.GetRegion(map, traversableRegionTypes);
			if (region == null)
			{
				return null;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, false);
			Region foundReg = null;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.portal != null)
				{
					return false;
				}
				if (tempRange.Includes(r.Room.Temperature))
				{
					foundReg = r;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, traversableRegionTypes);
			return foundReg;
		}
	}
}
