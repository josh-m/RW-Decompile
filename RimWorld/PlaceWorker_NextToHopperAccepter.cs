using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NextToHopperAccepter : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = loc + GenAdj.CardinalDirections[i];
				if (c.InBounds(base.Map))
				{
					List<Thing> thingList = c.GetThingList(base.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Thing thing = thingList[j];
						ThingDef thingDef = GenConstruct.BuiltDefOf(thing.def) as ThingDef;
						if (thingDef != null && thingDef.building != null)
						{
							if (thingDef.building.wantsHopperAdjacent)
							{
								return true;
							}
						}
					}
				}
			}
			return "MustPlaceNextToHopperAccepter".Translate();
		}
	}
}
