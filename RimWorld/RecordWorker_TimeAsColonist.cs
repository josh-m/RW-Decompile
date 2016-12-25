using System;
using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeAsColonist : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.Faction == Faction.OfPlayer;
		}
	}
}
