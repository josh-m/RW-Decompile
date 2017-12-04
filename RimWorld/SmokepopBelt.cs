using System;
using Verse;

namespace RimWorld
{
	public class SmokepopBelt : Apparel
	{
		private float ApparelScorePerBeltRadius = 0.046f;

		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.externalViolence && dinfo.Weapon != null && dinfo.Weapon.IsRangedWeapon)
			{
				IntVec3 position = base.Wearer.Position;
				Map map = base.Wearer.Map;
				float statValue = this.GetStatValue(StatDefOf.SmokepopBeltRadius, true);
				DamageDef smoke = DamageDefOf.Smoke;
				Thing instigator = null;
				ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
				GenExplosion.DoExplosion(position, map, statValue, smoke, instigator, -1, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
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
