using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_WealthBuildings : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return Find.StoryWatcher.watcherWealth.WealthBuildings;
		}
	}
}
