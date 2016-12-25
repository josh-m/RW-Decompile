using System;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_HeadOnShipBeam : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
		{
			IntVec3 c = loc + rot.FacingCell * -1;
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(base.Map);
			if (edifice == null || edifice.def != ThingDefOf.Ship_Beam)
			{
				return "MustPlaceHeadOnShipBeam".Translate();
			}
			return true;
		}
	}
}
