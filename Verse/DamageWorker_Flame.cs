using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class DamageWorker_Flame : DamageWorker_AddInjury
	{
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			if (!dinfo.InstantOldInjury)
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			Map map = victim.Map;
			DamageWorker.DamageResult result = base.Apply(dinfo, victim);
			if (victim.Destroyed && map != null && pawn == null)
			{
				foreach (IntVec3 current in victim.OccupiedRect())
				{
					FilthMaker.MakeFilth(current, map, ThingDefOf.FilthAsh, 1);
				}
				Plant plant = victim as Plant;
				if (plant != null && victim.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Sowing && victim.def != ThingDefOf.BurnedTree)
				{
					DeadPlant deadPlant = (DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, victim.Position, map);
					deadPlant.Growth = plant.Growth;
				}
			}
			return result;
		}

		public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, bool canThrowMotes)
		{
			base.ExplosionAffectCell(explosion, c, damagedThings, canThrowMotes);
			if (this.def == DamageDefOf.Flame && Rand.Chance(FireUtility.ChanceToStartFireIn(c, explosion.Map)))
			{
				FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f));
			}
		}
	}
}
