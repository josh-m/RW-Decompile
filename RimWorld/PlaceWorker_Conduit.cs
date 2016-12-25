using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Conduit : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
		{
			List<Thing> thingList = loc.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.EverTransmitsPower)
				{
					return false;
				}
				if (thingList[i].def.entityDefToBuild != null)
				{
					ThingDef thingDef = thingList[i].def.entityDefToBuild as ThingDef;
					if (thingDef != null && thingDef.EverTransmitsPower)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
