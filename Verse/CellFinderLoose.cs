using System;

namespace Verse
{
	public static class CellFinderLoose
	{
		public static IntVec3 RandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries = 1000)
		{
			IntVec3 result;
			CellFinderLoose.TryGetRandomCellWith(validator, map, maxTries, out result);
			return result;
		}

		public static bool TryGetRandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries, out IntVec3 result)
		{
			for (int i = 0; i < maxTries; i++)
			{
				result = CellFinder.RandomCell(map);
				if (validator(result))
				{
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindRandomNotEdgeCellWith(int minEdgeDistance, Predicate<IntVec3> validator, Map map, out IntVec3 result)
		{
			for (int i = 0; i < 1000; i++)
			{
				result = CellFinder.RandomNotEdgeCell(minEdgeDistance, map);
				if (validator(result))
				{
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}
	}
}
