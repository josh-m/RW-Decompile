using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenExplosion
	{
		private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);

		public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool dealMoreDamageAtCenter = false)
		{
			if (map == null)
			{
				Log.Warning("Tried to do explosion in a null map.");
				return;
			}
			if (damAmount == 0)
			{
				damAmount = 1;
			}
			Explosion explosion = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
			explosion.radius = radius;
			explosion.damType = damType;
			explosion.instigator = instigator;
			explosion.damAmount = ((damAmount <= 0) ? damType.explosionDamage : damAmount);
			explosion.weapon = weapon;
			explosion.projectile = projectile;
			explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
			explosion.preExplosionSpawnChance = preExplosionSpawnChance;
			explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
			explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
			explosion.postExplosionSpawnChance = postExplosionSpawnChance;
			explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
			explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
			explosion.chanceToStartFire = chanceToStartFire;
			explosion.dealMoreDamageAtCenter = dealMoreDamageAtCenter;
			explosion.StartExplosion(explosionSound);
		}

		public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius)
		{
			GenDraw.DrawFieldEdges(DamageDefOf.Bomb.Worker.ExplosionCellsToHit(loc, Find.VisibleMap, radius).ToList<IntVec3>());
		}

		public static void NotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null)
		{
			if (!damage.externalViolence)
			{
				return;
			}
			Room room = exploder.GetRoom(RegionType.Set_Passable);
			for (int i = 0; i < GenExplosion.PawnNotifyCellCount; i++)
			{
				IntVec3 c = exploder.Position + GenRadial.RadialPattern[i];
				if (c.InBounds(exploder.Map))
				{
					List<Thing> thingList = c.GetThingList(exploder.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Pawn pawn = thingList[j] as Pawn;
						if (pawn != null && pawn.RaceProps.intelligence >= Intelligence.Humanlike && (onlyFaction == null || pawn.Faction == onlyFaction))
						{
							Room room2 = pawn.GetRoom(RegionType.Set_Passable);
							if (room2 == null || room2.CellCount == 1 || (room2 == room && GenSight.LineOfSight(exploder.Position, pawn.Position, exploder.Map, true, null, 0, 0)))
							{
								pawn.mindState.Notify_DangerousExploderAboutToExplode(exploder);
							}
						}
					}
				}
			}
		}
	}
}
