using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FireWatcher
	{
		private const int UpdateObservationsInterval = 426;

		private const float BaseDangerPerFire = 0.5f;

		private Map map;

		private float fireDanger = -1f;

		public float FireDanger
		{
			get
			{
				return this.fireDanger;
			}
		}

		public bool LargeFireDangerPresent
		{
			get
			{
				if (this.fireDanger < 0f)
				{
					this.UpdateObservations();
				}
				return this.fireDanger > 90f;
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
			this.fireDanger = 0f;
			List<Thing> list = this.map.listerThings.ThingsOfDef(ThingDefOf.Fire);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				this.fireDanger += 0.5f + fire.fireSize;
			}
		}
	}
}
