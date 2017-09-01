using System;
using System.Text;
using Verse;

namespace RimWorld
{
	public sealed class StoryWatcher : IExposable
	{
		public StatsRecord statsRecord = new StatsRecord();

		public StoryWatcher_RampUp watcherRampUp = new StoryWatcher_RampUp();

		public void StoryWatcherTick()
		{
			this.watcherRampUp.RampUpWatcherTick();
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<StatsRecord>(ref this.statsRecord, "statsRecord", new object[0]);
			Scribe_Deep.Look<StoryWatcher_RampUp>(ref this.watcherRampUp, "watcherRampUp", new object[0]);
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Watcher: ");
			stringBuilder.AppendLine("  RampUp: " + this.watcherRampUp.TotalThreatPointsFactor);
			return stringBuilder.ToString();
		}
	}
}
