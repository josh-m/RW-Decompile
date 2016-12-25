using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_HeatWave : IncidentWorker_MakeMapCondition
	{
		protected override bool CanFireNowSub()
		{
			return base.CanFireNowSub() && GenTemperature.SeasonalTemp >= 20f;
		}
	}
}
