using System;
using Verse;

namespace RimWorld
{
	public class PredatorThreat : IExposable
	{
		private const int ExpireAfterTicks = 600;

		public Pawn predator;

		public int lastAttackTicks;

		public bool Expired
		{
			get
			{
				return !this.predator.Spawned || Find.TickManager.TicksGame >= this.lastAttackTicks + 600;
			}
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Pawn>(ref this.predator, "predator", false);
			Scribe_Values.LookValue<int>(ref this.lastAttackTicks, "lastAttackTicks", 0, false);
		}
	}
}
