using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_PsychicEmanatorShipPartCrash : IncidentWorker_ShipPartCrash
	{
		protected override bool CanFireNowSub()
		{
			return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.PsychicDrone) && base.CanFireNowSub();
		}
	}
}
