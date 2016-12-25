using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Flashstorm : IncidentWorker
	{
		protected override bool CanFireNowSub()
		{
			return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.Flashstorm);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
			MapCondition_Flashstorm mapCondition_Flashstorm = (MapCondition_Flashstorm)MapConditionMaker.MakeCondition(MapConditionDefOf.Flashstorm, duration, 0);
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, mapCondition_Flashstorm.centerLocation.ToIntVec3, null);
			Find.MapConditionManager.RegisterCondition(mapCondition_Flashstorm);
			if (Find.WeatherManager.curWeather.rainRate > 0.1f)
			{
				Find.Storyteller.weatherDecider.StartNextWeather();
			}
			return true;
		}
	}
}
