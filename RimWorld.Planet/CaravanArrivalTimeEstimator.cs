using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanArrivalTimeEstimator
	{
		private const int CacheDuration = 100;

		private const int MaxIterations = 10000;

		private static int cacheTicks = -1;

		private static Caravan cachedForCaravan;

		private static int cachedForDest = -1;

		private static int cachedResult = -1;

		public static int EstimatedTicksToArrive(Caravan caravan, bool allowCaching)
		{
			if (allowCaching && caravan == CaravanArrivalTimeEstimator.cachedForCaravan && caravan.pather.Destination == CaravanArrivalTimeEstimator.cachedForDest && Find.TickManager.TicksGame - CaravanArrivalTimeEstimator.cacheTicks < 100)
			{
				return CaravanArrivalTimeEstimator.cachedResult;
			}
			int to;
			int result;
			if (!caravan.Spawned || !caravan.pather.Moving || caravan.pather.curPath == null)
			{
				to = -1;
				result = 0;
			}
			else
			{
				to = caravan.pather.Destination;
				result = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(caravan.Tile, to, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove, Find.TickManager.TicksAbs);
			}
			if (allowCaching)
			{
				CaravanArrivalTimeEstimator.cacheTicks = Find.TickManager.TicksGame;
				CaravanArrivalTimeEstimator.cachedForCaravan = caravan;
				CaravanArrivalTimeEstimator.cachedForDest = to;
				CaravanArrivalTimeEstimator.cachedResult = result;
			}
			return result;
		}

		public static int EstimatedTicksToArrive(int from, int to, Caravan caravan)
		{
			int result;
			using (WorldPath worldPath = Find.WorldPathFinder.FindPath(from, to, caravan, null))
			{
				if (worldPath == WorldPath.NotFound)
				{
					result = 0;
				}
				else
				{
					result = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(from, to, worldPath, 0f, CaravanTicksPerMoveUtility.GetTicksPerMove(caravan), Find.TickManager.TicksAbs);
				}
			}
			return result;
		}

		public static int EstimatedTicksToArrive(int from, int to, WorldPath path, float nextTileCostLeft, int caravanTicksPerMove, int curTicksAbs)
		{
			int num = 0;
			int num2 = from;
			int num3 = 0;
			int num4 = Mathf.CeilToInt(20000f) - 1;
			int num5 = 60000 - num4;
			int num6 = 0;
			int num7 = 0;
			int num8;
			if (CaravanRestUtility.WouldBeRestingAt(from, (long)curTicksAbs))
			{
				num += CaravanRestUtility.LeftRestTicksAt(from, (long)curTicksAbs);
				num8 = num5;
			}
			else
			{
				num8 = CaravanRestUtility.LeftNonRestTicksAt(from, (long)curTicksAbs);
			}
			while (true)
			{
				num7++;
				if (num7 >= 10000)
				{
					break;
				}
				if (num6 <= 0)
				{
					if (num2 == to)
					{
						return num;
					}
					bool flag = num3 == 0;
					int start = num2;
					num2 = path.Peek(num3);
					num3++;
					float num9;
					if (flag)
					{
						num9 = nextTileCostLeft;
					}
					else
					{
						int num10 = curTicksAbs + num;
						float yearPercent = (float)GenDate.DayOfYear((long)num10, 0f) / 60f;
						num9 = (float)Caravan_PathFollower.CostToMove(caravanTicksPerMove, start, num2, yearPercent);
					}
					num6 = Mathf.CeilToInt(num9 / 1f);
				}
				if (num8 < num6)
				{
					num += num8;
					num6 -= num8;
					num += num4;
					num8 = num5;
				}
				else
				{
					num += num6;
					num8 -= num6;
					num6 = 0;
				}
			}
			Log.ErrorOnce("Could not calculate estimated ticks to arrive. Too many iterations.", 1837451324);
			return num;
		}
	}
}
