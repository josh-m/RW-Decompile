using System;
using Verse;

namespace RimWorld
{
	public static class GenThing
	{
		public static bool TryDropAndSetForbidden(Thing th, IntVec3 pos, Map map, ThingPlaceMode mode, out Thing resultingThing, bool forbidden)
		{
			if (GenDrop.TryDropSpawn(th, pos, map, ThingPlaceMode.Near, out resultingThing, null) && resultingThing != null)
			{
				resultingThing.SetForbidden(forbidden, false);
				return true;
			}
			resultingThing = null;
			return false;
		}
	}
}
