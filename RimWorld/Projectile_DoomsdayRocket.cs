using System;
using Verse;

namespace RimWorld
{
	public class Projectile_DoomsdayRocket : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			base.Impact(hitThing);
			IntVec3 position = base.Position;
			Map map2 = map;
			float explosionRadius = this.def.projectile.explosionRadius;
			DamageDef bomb = DamageDefOf.Bomb;
			Thing launcher = this.launcher;
			int damageAmountBase = this.def.projectile.damageAmountBase;
			ThingDef equipmentDef = this.equipmentDef;
			GenExplosion.DoExplosion(position, map2, explosionRadius, bomb, launcher, damageAmountBase, null, equipmentDef, this.def, null, 0f, 1, false, null, 0f, 1, 0f, false);
			CellRect cellRect = CellRect.CenteredOn(base.Position, 10);
			cellRect.ClipInsideMap(map);
			for (int i = 0; i < 5; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				this.FireExplosion(randomCell, map, 3.9f);
			}
		}

		protected void FireExplosion(IntVec3 pos, Map map, float radius)
		{
			DamageDef flame = DamageDefOf.Flame;
			Thing launcher = this.launcher;
			int damageAmountBase = this.def.projectile.damageAmountBase;
			ThingDef filthFuel = ThingDefOf.FilthFuel;
			GenExplosion.DoExplosion(pos, map, radius, flame, launcher, damageAmountBase, null, this.equipmentDef, this.def, filthFuel, 0.2f, 1, false, null, 0f, 1, 0f, false);
		}
	}
}
