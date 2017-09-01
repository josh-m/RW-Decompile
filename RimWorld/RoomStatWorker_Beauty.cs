using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Beauty : RoomStatWorker
	{
		private static readonly SimpleCurve CellCountCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 20f),
				true
			},
			{
				new CurvePoint(40f, 40f),
				true
			},
			{
				new CurvePoint(100000f, 100000f),
				true
			}
		};

		private static List<Thing> countedThings = new List<Thing>();

		private static List<IntVec3> countedAdjCells = new List<IntVec3>();

		public override float GetScore(Room room)
		{
			float num = 0f;
			int num2 = 0;
			RoomStatWorker_Beauty.countedThings.Clear();
			foreach (IntVec3 current in room.Cells)
			{
				num += BeautyUtility.CellBeauty(current, room.Map, RoomStatWorker_Beauty.countedThings);
				num2++;
			}
			RoomStatWorker_Beauty.countedAdjCells.Clear();
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				if (thing.GetRoom(RegionType.Set_Passable) != room && !RoomStatWorker_Beauty.countedAdjCells.Contains(thing.Position))
				{
					num += BeautyUtility.CellBeauty(thing.Position, room.Map, RoomStatWorker_Beauty.countedThings);
					RoomStatWorker_Beauty.countedAdjCells.Add(thing.Position);
				}
			}
			RoomStatWorker_Beauty.countedThings.Clear();
			if (num2 == 0)
			{
				return 0f;
			}
			return num / RoomStatWorker_Beauty.CellCountCurve.Evaluate((float)num2);
		}
	}
}
