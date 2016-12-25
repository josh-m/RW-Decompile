using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FactionTacticalMemory : IExposable
	{
		private const float TrapRememberChance = 0.2f;

		private List<TrapMemory> traps = new List<TrapMemory>();

		public void ExposeData()
		{
			Scribe_Collections.LookList<TrapMemory>(ref this.traps, "traps", LookMode.Deep, new object[0]);
		}

		public void Notify_MapRemoved(Map map)
		{
			this.traps.RemoveAll((TrapMemory x) => x.map == map);
		}

		public List<TrapMemory> TrapMemories()
		{
			this.traps.RemoveAll((TrapMemory tl) => tl.Expired);
			return this.traps;
		}

		public void TrapRevealed(IntVec3 c, Map map)
		{
			if (Rand.Value < 0.2f)
			{
				this.traps.Add(new TrapMemory(c, map, Find.TickManager.TicksGame));
			}
		}
	}
}
