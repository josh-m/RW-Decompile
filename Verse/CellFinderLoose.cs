using System;

namespace Verse
{
	public static class CellFinderLoose
	{
		public static IntVec3 RandomCellWith(Predicate<IntVec3> validator, int maxTries = 1000)
		{
			IntVec3 result;
			CellFinderLoose.TryGetRandomCellWith(validator, maxTries, out result);
			return result;
		}

		public static bool TryGetRandomCellWith(Predicate<IntVec3> validator, int maxTries, out IntVec3 result)
		{
			for (int i = 0; i < maxTries; i++)
			{
				result = CellFinder.RandomCell();
				if (validator(result))
				{
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindRandomNotEdgeCellWith(int minEdgeDistance, Predicate<IntVec3> validator, out IntVec3 result)
		{
			for (int i = 0; i < 1000; i++)
			{
				result = CellFinder.RandomNotEdgeCell(minEdgeDistance);
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
