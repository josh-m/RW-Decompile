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
			if (!caravan.Spawned || !caravan.pather.Moving || caravan.pather.curPath == null)
			{
				CaravanArrivalTimeEstimator.cacheTicks = Find.TickManager.TicksGame;
				CaravanArrivalTimeEstimator.cachedForCaravan = caravan;
				CaravanArrivalTimeEstimator.cachedForDest = -1;
				CaravanArrivalTimeEstimator.cachedResult = 0;
				return 0;
			}
			int num = 0;
			int num2 = caravan.Tile;
			int num3 = 0;
			int num4 = Mathf.CeilToInt(20000f) - 1;
			int num5 = 60000 - num4;
			int num6 = 0;
			int num7 = 0;
			int num8;
			if (caravan.Resting)
			{
				num += caravan.LeftRestTicks;
				num8 = num5;
			}
			else
			{
				num8 = caravan.LeftNonRestTicks;
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
					if (num2 == caravan.pather.Destination)
					{
						goto Block_10;
					}
					bool flag = num3 == 0;
					num2 = caravan.pather.curPath.Peek(num3);
					num3++;
					float num9;
					if (flag)
					{
						num9 = caravan.pather.nextTileCostLeft;
					}
					else
					{
						int num10 = Find.TickManager.TicksAbs + num;
						float yearPercent = (float)GenDate.DayOfYear((long)num10, 0f) / 60f;
						num9 = (float)Caravan_PathFollower.CostToMoveIntoTile(caravan, num2, yearPercent);
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
			CaravanArrivalTimeEstimator.cacheTicks = Find.TickManager.TicksGame;
			CaravanArrivalTimeEstimator.cachedForCaravan = caravan;
			CaravanArrivalTimeEstimator.cachedForDest = caravan.pather.Destination;
			CaravanArrivalTimeEstimator.cachedResult = num;
			return num;
			Block_10:
			CaravanArrivalTimeEstimator.cacheTicks = Find.TickManager.TicksGame;
			CaravanArrivalTimeEstimator.cachedForCaravan = caravan;
			CaravanArrivalTimeEstimator.cachedForDest = caravan.pather.Destination;
			CaravanArrivalTimeEstimator.cachedResult = num;
			return num;
		}
	}
}
