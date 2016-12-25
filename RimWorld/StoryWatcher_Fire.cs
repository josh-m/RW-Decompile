using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StoryWatcher_Fire
	{
		private const float DangerPerFire = 0.015f;

		private const float FireSizeFactor = 0.01f;

		private float fireAmount;

		public float FireAmount
		{
			get
			{
				return this.fireAmount;
			}
		}

		public bool LargeFireDangerPresent
		{
			get
			{
				return this.fireAmount > 2.3f;
			}
		}

		public void UpdateObservations()
		{
			this.fireAmount = 0f;
			List<Thing> list = Find.ListerThings.ThingsOfDef(ThingDefOf.Fire);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				this.fireAmount += 0.015f + 0.01f * fire.fireSize;
			}
		}
	}
}
