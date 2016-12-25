using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Flashstorm : IncidentWorker
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return !map.mapConditionManager.ConditionIsActive(MapConditionDefOf.Flashstorm);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
			MapCondition_Flashstorm mapCondition_Flashstorm = (MapCondition_Flashstorm)MapConditionMaker.MakeCondition(MapConditionDefOf.Flashstorm, duration, 0);
			map.mapConditionManager.RegisterCondition(mapCondition_Flashstorm);
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, new TargetInfo(mapCondition_Flashstorm.centerLocation.ToIntVec3, map, false), null);
			if (map.weatherManager.curWeather.rainRate > 0.1f)
			{
				map.weatherDecider.StartNextWeather();
			}
			return true;
		}
	}
}
