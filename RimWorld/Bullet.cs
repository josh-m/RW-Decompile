using System;
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
			if (hitThing != null)
			{
				int damageAmountBase = this.def.projectile.damageAmountBase;
				ThingDef equipmentDef = this.equipmentDef;
				DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef);
				hitThing.TakeDamage(dinfo);
			}
			else
			{
				SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(base.Position, map, false));
				MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
			}
		}
	}
}
