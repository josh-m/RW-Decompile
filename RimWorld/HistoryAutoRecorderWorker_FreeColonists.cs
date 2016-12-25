using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_FreeColonists : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return (float)Find.MapPawns.FreeColonistsCount;
		}
	}
}
