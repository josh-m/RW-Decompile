using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class BreakdownManager : MapComponent
	{
		public const int CheckIntervalTicks = 1041;

		private List<CompBreakdownable> comps = new List<CompBreakdownable>();

		public HashSet<Thing> brokenDownThings = new HashSet<Thing>();

		public BreakdownManager(Map map) : base(map)
		{
		}

		public void Register(CompBreakdownable c)
		{
			this.comps.Add(c);
			if (c.BrokenDown)
			{
				this.brokenDownThings.Add(c.parent);
			}
		}

		public void Deregister(CompBreakdownable c)
		{
			this.comps.Remove(c);
			this.brokenDownThings.Remove(c.parent);
		}

		public override void MapComponentTick()
		{
			if (Find.TickManager.TicksGame % 1041 == 0)
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					this.comps[i].CheckForBreakdown();
				}
			}
		}

		public void Notify_BrokenDown(Thing thing)
		{
			this.brokenDownThings.Add(thing);
		}

		public void Notify_Repaired(Thing thing)
		{
			this.brokenDownThings.Remove(thing);
		}
	}
}
