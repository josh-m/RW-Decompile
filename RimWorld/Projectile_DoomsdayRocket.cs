using System;
using Verse;

namespace RimWorld
{
	public class Projectile_DoomsdayRocket : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			base.Impact(hitThing);
			ThingDef def = this.def;
			GenExplosion.DoExplosion(base.Position, this.def.projectile.explosionRadius, DamageDefOf.Bomb, this.launcher, null, def, this.equipmentDef, null, 0f, 1, false, null, 0f, 1);
			CellRect cellRect = CellRect.CenteredOn(base.Position, 10);
			cellRect.ClipInsideMap();
			for (int i = 0; i < 5; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				this.FireExplosion(randomCell, 3.9f);
			}
		}

		protected void FireExplosion(IntVec3 pos, float radius)
		{
			ThingDef def = this.def;
			GenExplosion.DoExplosion(pos, radius, DamageDefOf.Flame, this.launcher, null, def, this.equipmentDef, ThingDefOf.FilthFuel, 0.2f, 1, false, null, 0f, 1);
		}
	}
}
