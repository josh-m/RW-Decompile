using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FireWatcher
	{
		private const float DangerPerFire = 0.015f;

		private const float FireSizeFactor = 0.01f;

		private const int UpdateObservationsInterval = 426;

		private Map map;

		private float fireAmount = -1f;

		public float FireAmount
		{
			get
			{
				if (this.fireAmount < 0f)
				{
					this.UpdateObservations();
				}
				return this.fireAmount;
			}
		}

		public bool LargeFireDangerPresent
		{
			get
			{
				if (this.fireAmount < 0f)
				{
					this.UpdateObservations();
				}
				return this.fireAmount > 2.3f;
			}
		}

		public FireWatcher(Map map)
		{
			this.map = map;
		}

		public void FireWatcherTick()
		{
			if (Find.TickManager.TicksGame % 426 == 0)
			{
				this.UpdateObservations();
			}
		}

		private void UpdateObservations()
		{
			this.fireAmount = 0f;
			List<Thing> list = this.map.listerThings.ThingsOfDef(ThingDefOf.Fire);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				this.fireAmount += 0.015f + 0.01f * fire.fireSize;
			}
		}
	}
}
