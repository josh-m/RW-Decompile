using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Kitchen : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> allContainedThings = room.AllContainedThings;
			for (int i = 0; i < allContainedThings.Count; i++)
			{
				Thing thing = allContainedThings[i];
				if (thing.def.designationCategory == DesignationCategoryDefOf.Production)
				{
					for (int j = 0; j < thing.def.AllRecipes.Count; j++)
					{
						RecipeDef recipeDef = thing.def.AllRecipes[j];
						for (int k = 0; k < recipeDef.products.Count; k++)
						{
							ThingDef thingDef = recipeDef.products[k].thingDef;
							if (thingDef.IsNutritionGivingIngestible && thingDef.ingestible.HumanEdible)
							{
								num++;
								goto IL_C7;
							}
						}
					}
				}
				IL_C7:;
			}
			return (float)num * 14f;
		}
	}
}
