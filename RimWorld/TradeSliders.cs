using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class TradeSliders
	{
		private const int DragTimeGrowMinimumOffset = 300;

		public static Tradeable dragTrad;

		public static int dragBaseAmount;

		public static bool dragLimitWarningGiven;

		private static Sustainer dragSustainer;

		private static float lastDragRealTime = -10000f;

		public static void TradeSliderDraggingStarted(float mouseOffX, float rateFactor)
		{
		}

		public static void TradeSliderDraggingUpdate(float mouseOffX, float rateFactor)
		{
			int num = TradeSliders.dragBaseAmount;
			if (Mathf.Abs(mouseOffX) > 300f)
			{
				if (mouseOffX > 0f)
				{
					TradeSliders.dragBaseAmount -= GenMath.RoundRandom(rateFactor);
				}
				else
				{
					TradeSliders.dragBaseAmount += GenMath.RoundRandom(rateFactor);
				}
			}
			int num2 = TradeSliders.dragBaseAmount - (int)(mouseOffX / 4f);
			int num3 = TradeSliders.dragTrad.countToDrop;
			AcceptanceReport acceptanceReport = null;
			while (num3 != num2)
			{
				if (num2 > num3)
				{
					acceptanceReport = TradeSliders.dragTrad.TrySetToTransferOneMoreToSource();
				}
				if (num2 < num3)
				{
					acceptanceReport = TradeSliders.dragTrad.TrySetToTransferOneMoreToDest();
				}
				if (!acceptanceReport.Accepted)
				{
					if (!TradeSliders.dragLimitWarningGiven)
					{
						if (TradeSliders.dragTrad.CountHeldBy(Transactor.Colony) + TradeSliders.dragTrad.CountHeldBy(Transactor.Trader) > 1)
						{
							Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
						}
						TradeSliders.dragLimitWarningGiven = true;
					}
					TradeSliders.dragBaseAmount = num;
					break;
				}
				if (num2 > num3)
				{
					num3++;
				}
				else
				{
					num3--;
				}
				TradeSliders.dragLimitWarningGiven = false;
			}
			if (acceptanceReport.Accepted)
			{
				if (TradeSliders.dragSustainer != null)
				{
					float num4 = -mouseOffX;
					if (num4 > 300f)
					{
						num4 = 300f;
					}
					if (num4 < -300f)
					{
						num4 = -300f;
					}
					TradeSliders.dragSustainer.externalParams["DragX"] = num4;
				}
				TradeSliders.lastDragRealTime = Time.realtimeSinceStartup;
			}
			if (TradeSliders.dragSustainer != null)
			{
				TradeSliders.dragSustainer.Maintain();
				TradeSliders.dragSustainer.externalParams["TimeSinceDrag"] = Time.realtimeSinceStartup - TradeSliders.lastDragRealTime;
			}
		}

		public static void TradeSliderDraggingCompleted(float mouseOffX, float rateFactor)
		{
			TradeSliders.dragSustainer = null;
		}
	}
}
