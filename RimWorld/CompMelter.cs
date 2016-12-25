using System;
using Verse;

namespace RimWorld
{
	public class CompMelter : ThingComp
	{
		private const float MeltPerIntervalPer10Degrees = 0.15f;

		public override void CompTickRare()
		{
			float temperature = this.parent.Position.GetTemperature(this.parent.Map);
			if (temperature < 0f)
			{
				return;
			}
			float f = 0.15f * (temperature / 10f);
			int num = GenMath.RoundRandom(f);
			if (num > 0)
			{
				this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, num, -1f, null, null, null));
			}
		}
	}
}
