using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Building_TrapDamager : Building_Trap
	{
		private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);

		private static readonly float DamageCount = 5f;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				SoundDefOf.TrapArm.PlayOneShot(new TargetInfo(base.Position, map, false));
			}
		}

		protected override void SpringSub(Pawn p)
		{
			SoundDefOf.TrapSpring.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			if (p != null)
			{
				float num = this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * Building_TrapDamager.DamageRandomFactorRange.RandomInRange;
				float num2 = num / Building_TrapDamager.DamageCount;
				float armorPenetration = num2 * 0.015f;
				int num3 = 0;
				while ((float)num3 < Building_TrapDamager.DamageCount)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, num2, armorPenetration, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
					p.TakeDamage(dinfo);
					num3++;
				}
			}
		}
	}
}
