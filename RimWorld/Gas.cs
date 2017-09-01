using System;
using Verse;

namespace RimWorld
{
	public class Gas : Thing
	{
		public int destroyTick;

		public float graphicRotation;

		public float graphicRotationSpeed;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			while (true)
			{
				Thing gas = base.Position.GetGas(map);
				if (gas == null)
				{
					break;
				}
				gas.Destroy(DestroyMode.Vanish);
			}
			base.SpawnSetup(map, respawningAfterLoad);
			this.destroyTick = Find.TickManager.TicksGame + this.def.gas.expireSeconds.RandomInRange.SecondsToTicks();
			this.graphicRotationSpeed = Rand.Range(-this.def.gas.rotationSpeed, this.def.gas.rotationSpeed) / 60f;
		}

		public override void Tick()
		{
			if (this.destroyTick <= Find.TickManager.TicksGame)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			this.graphicRotation += this.graphicRotationSpeed;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.destroyTick, "destroyTick", 0, false);
		}
	}
}
