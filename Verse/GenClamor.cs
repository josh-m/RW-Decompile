using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GenClamor
	{
		public static void DoClamor(Thing source, float radius, ClamorDef type)
		{
			IntVec3 root = source.Position;
			Region region = source.GetRegion(RegionType.Set_Passable);
			if (region == null)
			{
				return;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null || r.door.Open, delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i] as Pawn;
					float num = Mathf.Clamp01(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing));
					if (num > 0f && pawn.Position.InHorDistOf(root, radius * num))
					{
						pawn.HearClamor(source, type);
					}
				}
				return false;
			}, 15, RegionType.Set_Passable);
		}
	}
}
