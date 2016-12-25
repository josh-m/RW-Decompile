using System;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_HeadOnShipBeam : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
		{
			IntVec3 c = loc + rot.FacingCell * -1;
			if (!c.InBounds())
			{
				return false;
			}
			Building edifice = c.GetEdifice();
			if (edifice == null || edifice.def != ThingDefOf.Ship_Beam)
			{
				return "MustPlaceHeadOnShipBeam".Translate();
			}
			return true;
		}
	}
}
