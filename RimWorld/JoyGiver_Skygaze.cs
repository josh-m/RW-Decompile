using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_Skygaze : JoyGiver
	{
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
