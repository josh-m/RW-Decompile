using System;

namespace Verse
{
	public abstract class PlaceWorker
	{
		public virtual AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
		{
			return AcceptanceReport.WasAccepted;
		}

		public virtual void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
		}

		public virtual bool ForceAllowPlaceOver(BuildableDef other)
		{
			return false;
		}
	}
}
