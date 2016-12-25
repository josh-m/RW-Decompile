using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Beauty : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float num = 0f;
			int num2 = 0;
			foreach (IntVec3 current in room.Cells)
			{
				num += BeautyUtility.CellBeauty(current, null);
				num2++;
			}
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				if (allContainedThings[i].def.holdsRoof)
				{
					num += BeautyUtility.CellBeauty(allContainedThings[i].Position, null);
					num2++;
				}
			}
			if (num2 == 0)
			{
				return 0f;
			}
			return num / (float)num2;
		}
	}
}
