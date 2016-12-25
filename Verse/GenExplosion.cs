using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenExplosion
	{
		private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);

		public static void DoExplosion(IntVec3 center, float radius, DamageDef damType, Thing instigator, SoundDef explosionSound = null, ThingDef projectile = null, ThingDef source = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1)
		{
			Explosion explosion = new Explosion();
			explosion.position = center;
			explosion.radius = radius;
			explosion.damType = damType;
			explosion.instigator = instigator;
			explosion.damAmount = ((projectile == null) ? GenMath.RoundRandom((float)damType.explosionDamage) : projectile.projectile.damageAmountBase);
			explosion.source = source;
			explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
			explosion.preExplosionSpawnChance = preExplosionSpawnChance;
			explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
			explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
			explosion.postExplosionSpawnChance = postExplosionSpawnChance;
			explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
			explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
			Find.Map.GetComponent<ExplosionManager>().StartExplosion(explosion, explosionSound);
		}

		public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius)
		{
			GenDraw.DrawFieldEdges(DamageDefOf.Bomb.Worker.ExplosionCellsToHit(loc, radius).ToList<IntVec3>());
		}

		public static void NotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null)
		{
			if (!damage.externalViolence)
			{
				return;
			}
			Room room = exploder.GetRoom();
			for (int i = 0; i < GenExplosion.PawnNotifyCellCount; i++)
			{
				IntVec3 c = exploder.Position + GenRadial.RadialPattern[i];
				if (c.InBounds())
				{
					List<Thing> thingList = c.GetThingList();
					for (int j = 0; j < thingList.Count; j++)
					{
						Pawn pawn = thingList[j] as Pawn;
						if (pawn != null && pawn.RaceProps.intelligence >= Intelligence.Humanlike && (onlyFaction == null || pawn.Faction == onlyFaction))
						{
							Room room2 = pawn.Position.GetRoom();
							if (room2 == null || room2.CellCount == 1 || (room2 == room && GenSight.LineOfSight(exploder.Position, pawn.Position, true)))
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
