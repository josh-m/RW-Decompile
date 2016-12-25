using RimWorld;
using System;

namespace Verse
{
	public class MentalStateWorker
	{
		public MentalStateDef def;

		public virtual bool StateCanOccur(Pawn pawn)
		{
			return (this.def.unspawnedCanDo || pawn.Spawned) && (this.def.prisonersCanDo || pawn.HostFaction == null) && (!this.def.colonistsOnly || pawn.Faction == Faction.OfPlayer);
		}
	}
}
