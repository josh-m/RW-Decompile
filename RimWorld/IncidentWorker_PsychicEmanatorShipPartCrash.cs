using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_PsychicEmanatorShipPartCrash : IncidentWorker_ShipPartCrash
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicDrone) && base.CanFireNowSub(target);
		}
	}
}
