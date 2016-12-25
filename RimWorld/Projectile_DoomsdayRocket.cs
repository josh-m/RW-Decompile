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
			ThingDef def = this.def;
			GenExplosion.DoExplosion(base.Position, map, this.def.projectile.explosionRadius, DamageDefOf.Bomb, this.launcher, null, def, this.equipmentDef, null, 0f, 1, false, null, 0f, 1);
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
			ThingDef def = this.def;
			GenExplosion.DoExplosion(pos, map, radius, DamageDefOf.Flame, this.launcher, null, def, this.equipmentDef, ThingDefOf.FilthFuel, 0.2f, 1, false, null, 0f, 1);
		}
	}
}
