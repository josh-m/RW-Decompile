using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SelfDefenseUtility
	{
		public const float FleeWhenDistToHostileLessThan = 8f;

		public static bool ShouldStartFleeing(Pawn pawn)
		{
			bool foundThreat = false;
			Region region = pawn.GetRegion();
			if (region == null)
			{
				return false;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.portal == null || reg.portal.Open, delegate(Region reg)
			{
				List<Thing> list = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != pawn && list[i].HostileTo(pawn) && !((IAttackTarget)list[i]).ThreatDisabled() && GenSight.LineOfSight(pawn.Position, list[i].Position, pawn.Map, false) && list[i].Position.DistanceToSquared(pawn.Position) < 64f)
					{
						foundThreat = true;
					}
				}
				return foundThreat;
			}, 9);
			return foundThreat;
		}
	}
}
