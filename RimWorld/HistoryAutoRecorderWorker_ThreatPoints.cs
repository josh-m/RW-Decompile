using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_ThreatPoints : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return (Find.AnyPlayerHomeMap == null) ? 0f : (StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap) / 10f);
		}
	}
}
