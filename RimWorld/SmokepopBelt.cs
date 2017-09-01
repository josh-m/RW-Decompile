using System;
using Verse;

namespace RimWorld
{
	public class SmokepopBelt : Apparel
	{
		private float ApparelScorePerBeltRadius = 0.046f;

		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.externalViolence && dinfo.WeaponGear != null && dinfo.WeaponGear.IsRangedWeapon)
			{
				ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
				GenExplosion.DoExplosion(base.Wearer.Position, base.Wearer.Map, this.GetStatValue(StatDefOf.SmokepopBeltRadius, true), DamageDefOf.Smoke, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1);
				this.Destroy(DestroyMode.Vanish);
			}
			return false;
		}

		public override float GetSpecialApparelScoreOffset()
		{
			return this.GetStatValue(StatDefOf.SmokepopBeltRadius, true) * this.ApparelScorePerBeltRadius;
		}
	}
}
