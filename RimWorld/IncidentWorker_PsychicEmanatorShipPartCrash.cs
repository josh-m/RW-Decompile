using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_PsychicEmanatorShipPartCrash : IncidentWorker_ShipPartCrash
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return !map.mapConditionManager.ConditionIsActive(MapConditionDefOf.PsychicDrone) && base.CanFireNowSub(target);
		}
	}
}
