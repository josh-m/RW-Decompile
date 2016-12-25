using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Workshop : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				if (allContainedThings[i] is Building_WorkTable && allContainedThings[i].def.designationCategory == DesignationCategoryDefOf.Production)
				{
					num++;
				}
			}
			return 13.5f * (float)num;
		}
	}
}
