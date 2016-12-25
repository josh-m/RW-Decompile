using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_Prisoners : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return (float)Find.MapPawns.PrisonersOfColonyCount;
		}
	}
}
