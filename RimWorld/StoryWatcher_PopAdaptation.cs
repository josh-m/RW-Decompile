using System;
using Verse;

namespace RimWorld
{
	public class StoryWatcher_PopAdaptation : IExposable
	{
		private float adaptDays;

		private const int UpdateInterval = 30000;

		public float AdaptDays
		{
			get
			{
				return this.adaptDays;
			}
		}

		public void Notify_PawnEvent(Pawn p, PopAdaptationEvent ev)
		{
			if (!p.RaceProps.Humanlike)
			{
				return;
			}
			if (DebugViewSettings.writeStoryteller)
			{
				Log.Message(string.Concat(new object[]
				{
					"PopAdaptation event: ",
					ev,
					" - ",
					p
				}), false);
			}
			if (ev == PopAdaptationEvent.GainedColonist)
			{
				this.adaptDays = 0f;
			}
			else if (ev == PopAdaptationEvent.GainedPrisoner)
			{
				this.adaptDays /= 2f;
			}
		}

		public void PopAdaptationWatcherTick()
		{
			if (Find.TickManager.TicksGame % 30000 == 171)
			{
				float num = 0.5f;
				this.adaptDays += num;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<float>(ref this.adaptDays, "adaptDays", 0f, false);
		}

		public void Debug_OffsetAdaptDays(float days)
		{
			this.adaptDays += days;
		}
	}
}
