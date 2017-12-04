using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Aurora : IncidentWorker_MakeGameCondition
	{
		private const int EnsureMinDurationTicks = 5000;

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			if (!base.CanFireNowSub(target))
			{
				return false;
			}
			Map map = (Map)target;
			return GenCelestial.CurCelestialSunGlow(map) <= 0.5f && GenCelestial.CelestialSunGlow(map, Find.TickManager.TicksAbs + 5000) <= 0.5f;
		}
	}
}
