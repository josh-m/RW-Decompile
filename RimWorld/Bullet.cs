using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Bullet : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			base.Impact(hitThing);
			if (hitThing != null)
			{
				int damageAmountBase = this.def.projectile.damageAmountBase;
				BodyPartDamageInfo value = new BodyPartDamageInfo(null, null);
				DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(value), this.equipmentDef);
				hitThing.TakeDamage(dinfo);
			}
			else
			{
				SoundDefOf.BulletImpactGround.PlayOneShot(base.Position);
				MoteMaker.MakeStaticMote(this.ExactPosition, ThingDefOf.Mote_ShotHit_Dirt, 1f);
			}
		}
	}
}
