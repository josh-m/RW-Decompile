using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Laboratory : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				Thing thing = allContainedThings[i];
				if (thing is Building_ResearchBench)
				{
					num++;
				}
			}
			return 30f * (float)num;
		}
	}
}
