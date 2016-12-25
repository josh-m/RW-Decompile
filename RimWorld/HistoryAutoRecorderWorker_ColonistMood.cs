using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_ColonistMood : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			if (Find.MapPawns.FreeColonistsCount == 0)
			{
				return 0f;
			}
			return Find.MapPawns.FreeColonists.Average((Pawn x) => x.needs.mood.CurLevel * 100f);
		}
	}
}
