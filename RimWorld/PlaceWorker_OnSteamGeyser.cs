using System;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_OnSteamGeyser : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
		{
			Thing thing = base.Map.thingGrid.ThingAt(loc, ThingDefOf.SteamGeyser);
			if (thing == null || thing.Position != loc)
			{
				return "MustPlaceOnSteamGeyser".Translate();
			}
			return true;
		}

		public override bool ForceAllowPlaceOver(BuildableDef otherDef)
		{
			return otherDef == ThingDefOf.SteamGeyser;
		}
	}
}
