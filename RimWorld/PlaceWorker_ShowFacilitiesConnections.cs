using System;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowFacilitiesConnections : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			if (def.HasComp(typeof(CompAffectedByFacilities)))
			{
				CompAffectedByFacilities.DrawLinesToPotentialThingsToLinkTo(def, center, rot, base.Map);
			}
			else
			{
				CompFacility.DrawLinesToPotentialThingsToLinkTo(def, center, rot, base.Map);
			}
		}
	}
}
