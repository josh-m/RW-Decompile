using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Tomb : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				if (allContainedThings[i] is Building_Sarcophagus)
				{
					num++;
				}
			}
			return 50f * (float)num;
		}
	}
}
