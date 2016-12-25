using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_PoisonShipPartCrash : IncidentWorker_ShipPartCrash
	{
		protected override int CountToSpawn
		{
			get
			{
				return Rand.RangeInclusive(1, 1);
			}
		}
	}
}
