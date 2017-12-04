using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Bullet : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			base.Impact(hitThing);
			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget, this.equipmentDef, this.def);
			Find.BattleLog.Add(battleLogEntry_RangedImpact);
			if (hitThing != null)
			{
				int damageAmountBase = this.def.projectile.damageAmountBase;
				DamageDef damageDef = this.def.projectile.damageDef;
				int amount = damageAmountBase;
				float y = this.ExactRotation.eulerAngles.y;
				Thing launcher = this.launcher;
				ThingDef equipmentDef = this.equipmentDef;
				DamageInfo dinfo = new DamageInfo(damageDef, amount, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
				hitThing.TakeDamage(dinfo).InsertIntoLog(battleLogEntry_RangedImpact);
			}
			else
			{
				SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(base.Position, map, false));
				MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
				if (base.Position.GetTerrain(map).takeSplashes)
				{
					MoteMaker.MakeWaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)this.def.projectile.damageAmountBase) * 1f, 4f);
				}
			}
		}
	}
}
