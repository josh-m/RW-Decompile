using System;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_ResearchSpeedFactor : RoomStatWorker
	{
		private static readonly SimpleCurve CleanlinessFactorCurve = new SimpleCurve
		{
			new CurvePoint(-5f, 0.75f),
			new CurvePoint(-2.5f, 0.85f),
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 1.15f)
		};

		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Cleanliness);
			return RoomStatWorker_ResearchSpeedFactor.CleanlinessFactorCurve.Evaluate(stat);
		}
	}
}
