using System;
using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeAsColonistOrColonyAnimal : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.Faction == Faction.OfPlayer;
		}
	}
}
