using System;
using System.Text;
using Verse;

namespace RimWorld
{
	public sealed class StoryWatcher : IExposable
	{
		private const int TickMod = 15612;

		private const int CheckInterval = 426;

		public StatsRecord statsRecord = new StatsRecord();

		public StoryState storyState = new StoryState();

		public StoryWatcher_Fire watcherFire = new StoryWatcher_Fire();

		public StoryWatcher_Strength watcherStrength = new StoryWatcher_Strength();

		public StoryWatcher_Danger watcherDanger = new StoryWatcher_Danger();

		public StoryWatcher_Wealth watcherWealth = new StoryWatcher_Wealth();

		public StoryWatcher_Damage watcherDamage = new StoryWatcher_Damage();

		public StoryWatcher_RampUp watcherRampUp = new StoryWatcher_RampUp();

		public void StoryWatcherTick()
		{
			if ((Find.TickManager.TicksGame + 15612) % 426 == 0)
			{
				this.watcherFire.UpdateObservations();
			}
			this.watcherDamage.DamageWatcherTick();
			this.watcherRampUp.RampUpWatcherTick();
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<StatsRecord>(ref this.statsRecord, "statsRecord", new object[0]);
			Scribe_Deep.LookDeep<StoryState>(ref this.storyState, "storyState", new object[0]);
			Scribe_Deep.LookDeep<StoryWatcher_Damage>(ref this.watcherDamage, "watcherDamage", new object[0]);
			Scribe_Deep.LookDeep<StoryWatcher_RampUp>(ref this.watcherRampUp, "watcherRampUp", new object[0]);
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Watcher: ");
			stringBuilder.AppendLine("  FireAmount: " + this.watcherFire.FireAmount.ToString("####0.00"));
			if (this.watcherFire.LargeFireDangerPresent)
			{
				stringBuilder.AppendLine("    Fire danger present");
			}
			stringBuilder.AppendLine("  Strength: " + this.watcherStrength.StrengthRating.ToString("####0.00"));
			stringBuilder.AppendLine("  Danger: " + this.watcherDanger.DangerRating);
			stringBuilder.AppendLine("  Damage: " + this.watcherDamage.DamageTakenRecently);
			stringBuilder.AppendLine("  Wealth: " + this.watcherWealth.WealthTotal);
			stringBuilder.AppendLine("  RampUp: " + this.watcherRampUp.TotalThreatPointsFactor);
			return stringBuilder.ToString();
		}
	}
}
