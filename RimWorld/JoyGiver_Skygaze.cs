using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_Skygaze : JoyGiver
	{
		public override float GetChance(Pawn pawn)
		{
			float num = 1f;
			List<GameCondition> activeConditions = pawn.Map.gameConditionManager.ActiveConditions;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num *= activeConditions[i].SkyGazeChanceFactor;
			}
			activeConditions = Find.World.gameConditionManager.ActiveConditions;
			for (int j = 0; j < activeConditions.Count; j++)
			{
				num *= activeConditions[j].SkyGazeChanceFactor;
			}
			return base.GetChance(pawn) * num;
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn, null) || pawn.Map.weatherManager.curWeather.rainRate > 0.1f)
			{
				return null;
			}
			IntVec3 c;
			if (!RCellFinder.TryFindSkygazeCell(pawn.Position, pawn, out c))
			{
				return null;
			}
			return new Job(this.def.jobDef, c);
		}
	}
}
