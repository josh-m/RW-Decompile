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
			if (hitThing != null)
			{
				int damageAmountBase = this.def.projectile.damageAmountBase;
				ThingDef equipmentDef = this.equipmentDef;
				DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
				hitThing.TakeDamage(dinfo);
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
