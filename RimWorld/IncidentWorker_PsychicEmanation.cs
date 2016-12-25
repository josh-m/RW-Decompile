using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class IncidentWorker_PsychicEmanation : IncidentWorker
	{
		protected override bool CanFireNowSub()
		{
			return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.PsychicDrone) && !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.PsychicSoothe) && Find.ListerThings.ThingsOfDef(ThingDefOf.CrashedPsychicEmanatorShipPart).Count <= 0 && Find.MapPawns.FreeColonistsCount != 0;
		}

		public override bool TryExecute(IncidentParms parms)
		{
			this.DoConditionAndLetter(Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f), Find.MapPawns.FreeColonists.RandomElement<Pawn>().gender);
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
			return true;
		}

		protected abstract void DoConditionAndLetter(int duration, Gender gender);
	}
}
