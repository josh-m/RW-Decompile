using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_WealthItems : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return Find.StoryWatcher.watcherWealth.WealthItems;
		}
	}
}
