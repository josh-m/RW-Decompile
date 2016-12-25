using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class WalkPathFinder
	{
		private const int NumPathNodes = 8;

		private const float StepDistMin = 2f;

		private const float StepDistMax = 14f;

		private static readonly int StartRadialIndex = GenRadial.NumCellsInRadius(14f);

		private static readonly int EndRadialIndex = GenRadial.NumCellsInRadius(2f);

		private static readonly int RadialIndexStride = 3;

		public static bool TryFindWalkPath(Pawn pawn, IntVec3 root, out List<IntVec3> result)
		{
			List<IntVec3> list = new List<IntVec3>();
			list.Add(root);
			IntVec3 intVec = root;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec2 = IntVec3.Invalid;
				float num = -1f;
				for (int j = WalkPathFinder.StartRadialIndex; j > WalkPathFinder.EndRadialIndex; j -= WalkPathFinder.RadialIndexStride)
				{
					IntVec3 intVec3 = intVec + GenRadial.RadialPattern[j];
					if (intVec3.InBounds(pawn.Map) && intVec3.Standable(pawn.Map) && !intVec3.IsForbidden(pawn) && GenSight.LineOfSight(intVec, intVec3, pawn.Map, false) && !intVec3.Roofed(pawn.Map))
					{
						float num2 = 10000f;
						for (int k = 0; k < list.Count; k++)
						{
							num2 += (float)(list[k] - intVec3).LengthManhattan;
						}
						float num3 = (float)(intVec3 - root).LengthManhattan;
						if (num3 > 40f)
						{
							num2 *= Mathf.InverseLerp(70f, 40f, num3);
						}
						if (list.Count >= 2)
						{
							float num4 = (list[list.Count - 1] - list[list.Count - 2]).AngleFlat;
							float angleFlat = (intVec3 - intVec).AngleFlat;
							float num5;
							if (angleFlat > num4)
							{
								num5 = angleFlat - num4;
							}
							else
							{
								num4 -= 360f;
								num5 = angleFlat - num4;
							}
							if (num5 > 110f)
							{
								num2 *= 0.01f;
							}
						}
						if (list.Count >= 4 && (intVec - root).LengthManhattan < (intVec3 - root).LengthManhattan)
						{
							num2 *= 1E-05f;
						}
						if (num2 > num)
						{
							intVec2 = intVec3;
							num = num2;
						}
					}
				}
				if (num < 0f)
				{
					result = null;
					return false;
				}
				list.Add(intVec2);
				intVec = intVec2;
			}
			list.Add(root);
			result = list;
			return true;
		}

		public static void DebugFlashWalkPath(IntVec3 root, int numEntries = 8)
		{
			Map visibleMap = Find.VisibleMap;
			List<IntVec3> list;
			if (!WalkPathFinder.TryFindWalkPath(visibleMap.mapPawns.FreeColonistsSpawned.First<Pawn>(), root, out list))
			{
				visibleMap.debugDrawer.FlashCell(root, 0.2f, "NOPATH");
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				visibleMap.debugDrawer.FlashCell(list[i], (float)i / (float)numEntries, i.ToString());
				if (i > 0)
				{
					visibleMap.debugDrawer.FlashLine(list[i], list[i - 1]);
				}
			}
		}
	}
}
