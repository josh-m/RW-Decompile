using System;
using Verse;

namespace RimWorld
{
	public class LiquidFuel : Filth
	{
		private const int DryOutTime = 1500;

		private int spawnTick;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.spawnTick, "spawnTick", 0, false);
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.spawnTick = Find.TickManager.TicksGame;
		}

		public void Refill()
		{
			this.spawnTick = Find.TickManager.TicksGame;
		}

		public override void Tick()
		{
			if (this.spawnTick + 1500 < Find.TickManager.TicksGame)
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}

		public override void ThickenFilth()
		{
			base.ThickenFilth();
			this.Refill();
		}
	}
}
