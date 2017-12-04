using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalStateWorker
	{
		public MentalStateDef def;

		public virtual bool StateCanOccur(Pawn pawn)
		{
			if (!this.def.unspawnedCanDo && !pawn.Spawned)
			{
				return false;
			}
			if (!this.def.prisonersCanDo && pawn.HostFaction != null)
			{
				return false;
			}
			if (this.def.colonistsOnly && pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			for (int i = 0; i < this.def.requiredCapacities.Count; i++)
			{
				if (!pawn.health.capacities.CapableOf(this.def.requiredCapacities[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
