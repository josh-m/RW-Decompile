using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_ColonistMood : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			IEnumerable<Pawn> allMaps_FreeColonists = PawnsFinder.AllMaps_FreeColonists;
			if (!allMaps_FreeColonists.Any<Pawn>())
			{
				return 0f;
			}
			return allMaps_FreeColonists.Average((Pawn x) => x.needs.mood.CurLevel * 100f);
		}
	}
}
