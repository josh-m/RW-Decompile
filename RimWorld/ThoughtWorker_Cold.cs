using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Cold : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			float statValue = p.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			float temperatureAtCellOrCaravanTile = GenTemperature.GetTemperatureAtCellOrCaravanTile(p);
			float num = statValue - temperatureAtCellOrCaravanTile;
			if (num <= 0f)
			{
				return ThoughtState.Inactive;
			}
			if (num < 10f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (num < 20f)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			return ThoughtState.ActiveAtStage(2);
		}
	}
}
