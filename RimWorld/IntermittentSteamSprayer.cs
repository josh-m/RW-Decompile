using System;
using Verse;

namespace RimWorld
{
	public class IntermittentSteamSprayer
	{
		private const int MinTicksBetweenSprays = 500;

		private const int MaxTicksBetweenSprays = 2000;

		private const int MinSprayDuration = 200;

		private const int MaxSprayDuration = 500;

		private const float SprayThickness = 0.6f;

		private Thing parent;

		private int ticksUntilSpray = 500;

		private int sprayTicksLeft;

		public Action startSprayCallback;

		public Action endSprayCallback;

		public IntermittentSteamSprayer(Thing parent)
		{
			this.parent = parent;
		}

		public void SteamSprayerTick()
		{
			if (this.sprayTicksLeft > 0)
			{
				this.sprayTicksLeft--;
				if (Rand.Value < 0.6f)
				{
					MoteMaker.ThrowAirPuffUp(this.parent.TrueCenter(), this.parent.Map);
				}
				if (Find.TickManager.TicksGame % 20 == 0)
				{
					GenTemperature.PushHeat(this.parent, 40f);
				}
				if (this.sprayTicksLeft <= 0)
				{
					if (this.endSprayCallback != null)
					{
						this.endSprayCallback();
					}
					this.ticksUntilSpray = Rand.RangeInclusive(500, 2000);
				}
			}
			else
			{
				this.ticksUntilSpray--;
				if (this.ticksUntilSpray <= 0)
				{
					if (this.startSprayCallback != null)
					{
						this.startSprayCallback();
					}
					this.sprayTicksLeft = Rand.RangeInclusive(200, 500);
				}
			}
		}
	}
}
