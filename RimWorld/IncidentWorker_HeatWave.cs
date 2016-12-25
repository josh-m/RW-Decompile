using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_HeatWave : IncidentWorker_MakeMapCondition
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			if (!base.CanFireNowSub(target))
			{
				return false;
			}
			Map map = (Map)target;
			return map.mapTemperature.SeasonalTemp >= 20f;
		}
	}
}
