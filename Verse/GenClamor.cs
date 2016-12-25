using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GenClamor
	{
		public static void DoClamor(Pawn source, float radius, ClamorType type)
		{
			IntVec3 root = source.Position;
			Region region = source.GetRegion();
			if (region == null)
			{
				return;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.portal == null || r.portal.Open, delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i] as Pawn;
					if (pawn.Position.InHorDistOf(root, radius))
					{
						pawn.HearClamor(source, type);
					}
				}
				return false;
			}, 15);
		}
	}
}
