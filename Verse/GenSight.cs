using System;
using UnityEngine;

namespace Verse
{
	public static class GenSight
	{
		public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false)
		{
			if (!start.InBounds(map) || !end.InBounds(map))
			{
				return false;
			}
			if (start.AdjacentTo8Way(end) && (skipFirstCell || start.CanBeSeenOverFast(map)))
			{
				return true;
			}
			bool flag;
			if (start.x == end.x)
			{
				flag = (start.z < end.z);
			}
			else
			{
				flag = (start.x < end.x);
			}
			int num = Mathf.Abs(end.x - start.x);
			int num2 = Mathf.Abs(end.z - start.z);
			int num3 = start.x;
			int num4 = start.z;
			int i = 1 + num + num2;
			int num5 = (end.x <= start.x) ? -1 : 1;
			int num6 = (end.z <= start.z) ? -1 : 1;
			int num7 = num - num2;
			num *= 2;
			num2 *= 2;
			IntVec3 intVec = default(IntVec3);
			while (i > 1)
			{
				intVec.x = num3;
				intVec.z = num4;
				if ((!skipFirstCell || !(intVec == start)) && !intVec.CanBeSeenOverFast(map))
				{
					return false;
				}
				if ((float)num7 > 0f || ((float)num7 == 0f && flag))
				{
					num3 += num5;
					num7 -= num2;
				}
				else
				{
					num4 += num6;
					num7 += num;
				}
				i--;
			}
			return true;
		}

		public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, CellRect startRect, CellRect endRect)
		{
			if (!start.InBounds(map) || !end.InBounds(map))
			{
				return false;
			}
			if (start.AdjacentTo8Way(end) && (startRect.Contains(start) || endRect.Contains(start) || start.CanBeSeenOverFast(map)))
			{
				return true;
			}
			bool flag;
			if (start.x == end.x)
			{
				flag = (start.z < end.z);
			}
			else
			{
				flag = (start.x < end.x);
			}
			int num = Mathf.Abs(end.x - start.x);
			int num2 = Mathf.Abs(end.z - start.z);
			int num3 = start.x;
			int num4 = start.z;
			int i = 1 + num + num2;
			int num5 = (end.x <= start.x) ? -1 : 1;
			int num6 = (end.z <= start.z) ? -1 : 1;
			int num7 = num - num2;
			num *= 2;
			num2 *= 2;
			IntVec3 c = default(IntVec3);
			while (i > 1)
			{
				c.x = num3;
				c.z = num4;
				if (endRect.Contains(c))
				{
					return true;
				}
				if (!startRect.Contains(c) && !c.CanBeSeenOverFast(map))
				{
					return false;
				}
				if (num7 > 0 || ((float)num7 == 0f && flag))
				{
					num3 += num5;
					num7 -= num2;
				}
				else
				{
					num4 += num6;
					num7 += num;
				}
				i--;
			}
			return true;
		}
	}
}
