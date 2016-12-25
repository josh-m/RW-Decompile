using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class IncidentWorker_PsychicEmanation : IncidentWorker
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return !map.mapConditionManager.ConditionIsActive(MapConditionDefOf.PsychicDrone) && !map.mapConditionManager.ConditionIsActive(MapConditionDefOf.PsychicSoothe) && map.listerThings.ThingsOfDef(ThingDefOf.CrashedPsychicEmanatorShipPart).Count <= 0 && map.mapPawns.FreeColonistsCount != 0;
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			this.DoConditionAndLetter(map, Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f), map.mapPawns.FreeColonists.RandomElement<Pawn>().gender);
			if (map == Find.VisibleMap)
			{
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
			}
			return true;
		}

		protected abstract void DoConditionAndLetter(Map map, int duration, Gender gender);
	}
}
