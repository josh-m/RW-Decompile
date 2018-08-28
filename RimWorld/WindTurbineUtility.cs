using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class WindTurbineUtility
	{
		[DebuggerHidden]
		public static IEnumerable<IntVec3> CalculateWindCells(IntVec3 center, Rot4 rot, IntVec2 size)
		{
			CellRect rectA = default(CellRect);
			CellRect rectB = default(CellRect);
			int offset = 0;
			int neDist;
			int swDist;
			if (rot == Rot4.North || rot == Rot4.East)
			{
				neDist = 9;
				swDist = 5;
			}
			else
			{
				neDist = 5;
				swDist = 9;
				offset = -1;
			}
			if (rot.IsHorizontal)
			{
				rectA.minX = center.x + 2 + offset;
				rectA.maxX = center.x + 2 + neDist + offset;
				rectB.minX = center.x - 1 - swDist + offset;
				rectB.maxX = center.x - 1 + offset;
				rectB.minZ = (rectA.minZ = center.z - 3);
				rectB.maxZ = (rectA.maxZ = center.z + 3);
			}
			else
			{
				rectA.minZ = center.z + 2 + offset;
				rectA.maxZ = center.z + 2 + neDist + offset;
				rectB.minZ = center.z - 1 - swDist + offset;
				rectB.maxZ = center.z - 1 + offset;
				rectB.minX = (rectA.minX = center.x - 3);
				rectB.maxX = (rectA.maxX = center.x + 3);
			}
			for (int z = rectA.minZ; z <= rectA.maxZ; z++)
			{
				for (int x = rectA.minX; x <= rectA.maxX; x++)
				{
					yield return new IntVec3(x, 0, z);
				}
			}
			for (int z2 = rectB.minZ; z2 <= rectB.maxZ; z2++)
			{
				for (int x2 = rectB.minX; x2 <= rectB.maxX; x2++)
				{
					yield return new IntVec3(x2, 0, z2);
				}
			}
		}
	}
}
