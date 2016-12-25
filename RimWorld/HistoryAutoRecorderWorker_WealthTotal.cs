using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_WealthTotal : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return Find.StoryWatcher.watcherWealth.WealthTotal;
		}
	}
}
