using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Schedule : CompProperties
	{
		public float startTime;

		public float endTime = 1f;

		[MustTranslate]
		public string offMessage;

		public CompProperties_Schedule()
		{
			this.compClass = typeof(CompSchedule);
		}
	}
}
