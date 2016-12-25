using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Wealth : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float num = 0f;
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				Thing thing = allContainedThings[i];
				if (thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Plant)
				{
					num += (float)thing.stackCount * thing.GetInnerIfMinified().MarketValue;
				}
			}
			foreach (IntVec3 current in room.Cells)
			{
				num += current.GetTerrain(room.Map).GetStatValueAbstract(StatDefOf.MarketValue, null);
			}
			return num;
		}
	}
}
