using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Beauty : RoomStatWorker
	{
		private static readonly SimpleCurve CellCountCurve = new SimpleCurve
		{
			new CurvePoint(0f, 20f),
			new CurvePoint(40f, 40f),
			new CurvePoint(100000f, 100000f)
		};

		private static List<Thing> countedThings = new List<Thing>();

		public override float GetScore(Room room)
		{
			float num = 0f;
			int num2 = 0;
			foreach (IntVec3 current in room.Cells)
			{
				num += BeautyUtility.CellBeauty(current, room.Map, RoomStatWorker_Beauty.countedThings);
				num2++;
			}
			RoomStatWorker_Beauty.countedThings.Clear();
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				Thing thing = allContainedThings[i];
				if (thing.def.regionBarrier)
				{
					num += BeautyUtility.CellBeauty(thing.Position, room.Map, null);
				}
			}
			if (num2 == 0)
			{
				return 0f;
			}
			return num / RoomStatWorker_Beauty.CellCountCurve.Evaluate((float)num2);
		}
	}
}
