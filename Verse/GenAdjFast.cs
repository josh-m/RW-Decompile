using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GenAdjFast
	{
		private static List<IntVec3> resultList = new List<IntVec3>();

		private static bool working = false;

		public static List<IntVec3> AdjacentCells8Way(LocalTargetInfo pack)
		{
			if (pack.HasThing)
			{
				return GenAdjFast.AdjacentCells8Way((Thing)pack);
			}
			return GenAdjFast.AdjacentCells8Way((IntVec3)pack);
		}

		public static List<IntVec3> AdjacentCells8Way(IntVec3 root)
		{
			if (GenAdjFast.working)
			{
				throw new InvalidOperationException("GenAdjFast is already working.");
			}
			GenAdjFast.resultList.Clear();
			GenAdjFast.working = true;
			for (int i = 0; i < 8; i++)
			{
				GenAdjFast.resultList.Add(root + GenAdj.AdjacentCells[i]);
			}
			GenAdjFast.working = false;
			return GenAdjFast.resultList;
		}

		private static List<IntVec3> AdjacentCells8Way(Thing t)
		{
			return GenAdjFast.AdjacentCells8Way(t.Position, t.Rotation, t.def.size);
		}

		public static List<IntVec3> AdjacentCells8Way(IntVec3 thingCenter, Rot4 thingRot, IntVec2 thingSize)
		{
			if (thingSize.x == 1 && thingSize.z == 1)
			{
				return GenAdjFast.AdjacentCells8Way(thingCenter);
			}
			if (GenAdjFast.working)
			{
				throw new InvalidOperationException("GenAdjFast is already working.");
			}
			GenAdjFast.resultList.Clear();
			GenAdjFast.working = true;
			GenAdj.AdjustForRotation(ref thingCenter, ref thingSize, thingRot);
			int num = thingCenter.x - (thingSize.x - 1) / 2 - 1;
			int num2 = num + thingSize.x + 1;
			int num3 = thingCenter.z - (thingSize.z - 1) / 2 - 1;
			int num4 = num3 + thingSize.z + 1;
			IntVec3 item = new IntVec3(num - 1, 0, num3);
			do
			{
				item.x++;
				GenAdjFast.resultList.Add(item);
			}
			while (item.x < num2);
			do
			{
				item.z++;
				GenAdjFast.resultList.Add(item);
			}
			while (item.z < num4);
			do
			{
				item.x--;
				GenAdjFast.resultList.Add(item);
			}
			while (item.x > num);
			do
			{
				item.z--;
				GenAdjFast.resultList.Add(item);
			}
			while (item.z > num3 + 1);
			GenAdjFast.working = false;
			return GenAdjFast.resultList;
		}
	}
}
