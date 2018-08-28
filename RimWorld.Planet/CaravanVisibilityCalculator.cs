using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanVisibilityCalculator
	{
		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static readonly SimpleCurve BodySizeSumToVisibility = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(1f, 0.2f),
				true
			},
			{
				new CurvePoint(6f, 1f),
				true
			},
			{
				new CurvePoint(12f, 1.12f),
				true
			}
		};

		public const float NotMovingFactor = 0.3f;

		public static float Visibility(float bodySizeSum, bool caravanMovingNow, StringBuilder explanation = null)
		{
			float num = CaravanVisibilityCalculator.BodySizeSumToVisibility.Evaluate(bodySizeSum);
			if (explanation != null)
			{
				if (explanation.Length > 0)
				{
					explanation.AppendLine();
				}
				explanation.Append("TotalBodySize".Translate() + ": " + bodySizeSum.ToString("0.##"));
			}
			if (!caravanMovingNow)
			{
				num *= 0.3f;
				if (explanation != null)
				{
					explanation.AppendLine();
					explanation.Append("CaravanNotMoving".Translate() + ": " + 0.3f.ToStringPercent());
				}
			}
			return num;
		}

		public static float Visibility(Caravan caravan, StringBuilder explanation = null)
		{
			return CaravanVisibilityCalculator.Visibility(caravan.PawnsListForReading, caravan.pather.MovingNow, explanation);
		}

		public static float Visibility(List<Pawn> pawns, bool caravanMovingNow, StringBuilder explanation = null)
		{
			float num = 0f;
			for (int i = 0; i < pawns.Count; i++)
			{
				num += pawns[i].BodySize;
			}
			return CaravanVisibilityCalculator.Visibility(num, caravanMovingNow, explanation);
		}

		public static float Visibility(IEnumerable<Pawn> pawns, bool caravanMovingNow, StringBuilder explanation = null)
		{
			CaravanVisibilityCalculator.tmpPawns.Clear();
			CaravanVisibilityCalculator.tmpPawns.AddRange(pawns);
			float result = CaravanVisibilityCalculator.Visibility(CaravanVisibilityCalculator.tmpPawns, caravanMovingNow, explanation);
			CaravanVisibilityCalculator.tmpPawns.Clear();
			return result;
		}

		public static float Visibility(List<TransferableOneWay> transferables, StringBuilder explanation = null)
		{
			CaravanVisibilityCalculator.tmpPawns.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn)
				{
					for (int j = 0; j < transferableOneWay.CountToTransfer; j++)
					{
						CaravanVisibilityCalculator.tmpPawns.Add((Pawn)transferableOneWay.things[j]);
					}
				}
			}
			float result = CaravanVisibilityCalculator.Visibility(CaravanVisibilityCalculator.tmpPawns, true, explanation);
			CaravanVisibilityCalculator.tmpPawns.Clear();
			return result;
		}

		public static float VisibilityLeftAfterTransfer(List<TransferableOneWay> transferables, StringBuilder explanation = null)
		{
			CaravanVisibilityCalculator.tmpPawns.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn)
				{
					for (int j = transferableOneWay.things.Count - 1; j >= transferableOneWay.CountToTransfer; j--)
					{
						CaravanVisibilityCalculator.tmpPawns.Add((Pawn)transferableOneWay.things[j]);
					}
				}
			}
			float result = CaravanVisibilityCalculator.Visibility(CaravanVisibilityCalculator.tmpPawns, true, explanation);
			CaravanVisibilityCalculator.tmpPawns.Clear();
			return result;
		}

		public static float VisibilityLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, StringBuilder explanation = null)
		{
			CaravanVisibilityCalculator.tmpThingCounts.Clear();
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, CaravanVisibilityCalculator.tmpThingCounts);
			float result = CaravanVisibilityCalculator.Visibility(CaravanVisibilityCalculator.tmpThingCounts, explanation);
			CaravanVisibilityCalculator.tmpThingCounts.Clear();
			return result;
		}

		public static float Visibility(List<ThingCount> thingCounts, StringBuilder explanation = null)
		{
			CaravanVisibilityCalculator.tmpPawns.Clear();
			for (int i = 0; i < thingCounts.Count; i++)
			{
				if (thingCounts[i].Count > 0)
				{
					Pawn pawn = thingCounts[i].Thing as Pawn;
					if (pawn != null)
					{
						CaravanVisibilityCalculator.tmpPawns.Add(pawn);
					}
				}
			}
			float result = CaravanVisibilityCalculator.Visibility(CaravanVisibilityCalculator.tmpPawns, true, explanation);
			CaravanVisibilityCalculator.tmpPawns.Clear();
			return result;
		}
	}
}
