using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_Skygaze : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn, null) || Find.WeatherManager.curWeather.rainRate > 0.1f)
			{
				return null;
			}
			IntVec3 vec;
			if (!RCellFinder.TryFindSkygazeCell(pawn.Position, pawn, out vec))
			{
				return null;
			}
			return new Job(this.def.jobDef, vec);
		}
	}
}
