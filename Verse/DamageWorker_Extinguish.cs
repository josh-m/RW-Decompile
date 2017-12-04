using RimWorld;
using System;

namespace Verse
{
	public class DamageWorker_Extinguish : DamageWorker
	{
		private const float DamageAmountToFireSizeRatio = 0.01f;

		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageWorker.DamageResult result = DamageWorker.DamageResult.MakeNew();
			Fire fire = victim as Fire;
			if (fire == null || fire.Destroyed)
			{
				return result;
			}
			base.Apply(dinfo, victim);
			fire.fireSize -= (float)dinfo.Amount * 0.01f;
			if (fire.fireSize <= 0.1f)
			{
				fire.Destroy(DestroyMode.Vanish);
			}
			return result;
		}
	}
}
