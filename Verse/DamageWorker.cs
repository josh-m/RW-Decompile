using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class DamageWorker
	{
		public struct DamageResult
		{
			public bool wounded;

			public bool headshot;

			public bool deflected;

			public Thing hitThing;

			public List<BodyPartRecord> parts;

			public float totalDamageDealt;

			public BodyPartRecord LastHitPart
			{
				get
				{
					if (this.parts == null)
					{
						return null;
					}
					if (this.parts.Count <= 0)
					{
						return null;
					}
					return this.parts[this.parts.Count - 1];
				}
			}

			public static DamageWorker.DamageResult MakeNew()
			{
				return new DamageWorker.DamageResult
				{
					wounded = false,
					headshot = false,
					deflected = false,
					totalDamageDealt = 0f
				};
			}

			public void AddPart(Thing hitThing, BodyPartRecord part)
			{
				if (this.hitThing != null && this.hitThing != hitThing)
				{
					Log.ErrorOnce("Single damage worker referring to multiple things; will cause issues with combat log", 30667935);
				}
				this.hitThing = hitThing;
				if (this.parts == null)
				{
					this.parts = new List<BodyPartRecord>();
				}
				this.parts.Add(part);
			}

			public void InsertIntoLog(IDamageResultLog log)
			{
				if (this.hitThing == null)
				{
					return;
				}
				if (log == null)
				{
					return;
				}
				Pawn hitPawn = this.hitThing as Pawn;
				List<BodyPartDef> recipientParts = null;
				List<bool> recipientPartsDestroyed = null;
				if (!this.parts.NullOrEmpty<BodyPartRecord>() && hitPawn != null)
				{
					recipientParts = (from part in this.parts
					select part.def).Distinct<BodyPartDef>().ToList<BodyPartDef>();
					recipientPartsDestroyed = (from part in this.parts
					select hitPawn.health.hediffSet.GetPartHealth(part) <= 0f).ToList<bool>();
				}
				log.FillTargets(recipientParts, recipientPartsDestroyed);
			}
		}

		public DamageDef def;

		private const float ExplosionCamShakeMultiplier = 4f;

		private static List<Thing> thingsToAffect = new List<Thing>();

		private static List<IntVec3> openCells = new List<IntVec3>();

		private static List<IntVec3> adjWallCells = new List<IntVec3>();

		public virtual DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageWorker.DamageResult result = DamageWorker.DamageResult.MakeNew();
			if (victim.SpawnedOrAnyParentSpawned)
			{
				ImpactSoundUtility.PlayImpactSound(victim, dinfo.Def.impactSoundType, victim.MapHeld);
			}
			if (victim.def.useHitPoints && dinfo.Def.harmsHealth)
			{
				result.totalDamageDealt = (float)Mathf.Min(victim.HitPoints, dinfo.Amount);
				victim.HitPoints -= (int)result.totalDamageDealt;
				if (victim.HitPoints <= 0)
				{
					victim.HitPoints = 0;
					victim.Kill(new DamageInfo?(dinfo), null);
				}
			}
			return result;
		}

		public virtual void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
		{
			if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
			{
				GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
			}
			MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
			if (explosion.Map == Find.VisibleMap)
			{
				float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
				Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
			}
			this.ExplosionVisualEffectCenter(explosion);
		}

		protected virtual void ExplosionVisualEffectCenter(Explosion explosion)
		{
			for (int i = 0; i < 4; i++)
			{
				MoteMaker.ThrowSmoke(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, explosion.radius * 0.6f);
			}
			if (this.def.explosionInteriorMote != null)
			{
				int num = Mathf.RoundToInt(3.14159274f * explosion.radius * explosion.radius / 6f);
				for (int j = 0; j < num; j++)
				{
					MoteMaker.ThrowExplosionInteriorMote(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, this.def.explosionInteriorMote);
				}
			}
		}

		public virtual void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, bool canThrowMotes)
		{
			if (!c.InBounds(explosion.Map))
			{
				return;
			}
			if (this.def.explosionCellMote != null && canThrowMotes)
			{
				float t = Mathf.Clamp01((explosion.Position - c).LengthHorizontal / explosion.radius);
				Color color = Color.Lerp(this.def.explosionColorCenter, this.def.explosionColorEdge, t);
				MoteMaker.ThrowExplosionCell(c, explosion.Map, this.def.explosionCellMote, color);
			}
			List<Thing> list = explosion.Map.thingGrid.ThingsListAt(c);
			DamageWorker.thingsToAffect.Clear();
			float num = -3.40282347E+38f;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				DamageWorker.thingsToAffect.Add(thing);
				if (thing.def.Fillage == FillCategory.Full && thing.def.Altitude > num)
				{
					num = thing.def.Altitude;
				}
			}
			for (int j = 0; j < DamageWorker.thingsToAffect.Count; j++)
			{
				if (DamageWorker.thingsToAffect[j].def.Altitude >= num)
				{
					this.ExplosionDamageThing(explosion, DamageWorker.thingsToAffect[j], damagedThings, c);
				}
			}
			if (this.def.explosionSnowMeltAmount > 0.0001f)
			{
				float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
				float num2 = 1f - lengthHorizontal / explosion.radius;
				if (num2 > 0f)
				{
					explosion.Map.snowGrid.AddDepth(c, -num2 * this.def.explosionSnowMeltAmount);
				}
			}
			if (this.def == DamageDefOf.Bomb || this.def == DamageDefOf.Flame)
			{
				List<Thing> list2 = explosion.Map.listerThings.ThingsOfDef(ThingDefOf.RectTrigger);
				for (int k = 0; k < list2.Count; k++)
				{
					RectTrigger rectTrigger = (RectTrigger)list2[k];
					if (rectTrigger.activateOnExplosion && rectTrigger.Rect.Contains(c))
					{
						rectTrigger.ActivatedBy(null);
					}
				}
			}
		}

		protected virtual void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, IntVec3 cell)
		{
			if (t.def.category == ThingCategory.Mote)
			{
				return;
			}
			if (damagedThings.Contains(t))
			{
				return;
			}
			damagedThings.Add(t);
			if (this.def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
			{
				t.Destroy(DestroyMode.Vanish);
				return;
			}
			float num;
			if (t.Position == explosion.Position)
			{
				num = (float)Rand.RangeInclusive(0, 359);
			}
			else
			{
				num = (t.Position - explosion.Position).AngleFlat;
			}
			DamageDef damageDef = this.def;
			int amount = explosion.GetDamageAmountAt(cell);
			float angle = num;
			Thing instigator = explosion.instigator;
			ThingDef weapon = explosion.weapon;
			DamageInfo dinfo = new DamageInfo(damageDef, amount, angle, instigator, null, weapon, DamageInfo.SourceCategory.ThingOrUnknown);
			if (this.def.explosionAffectOutsidePartsOnly)
			{
				dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			}
			if (t.def.category == ThingCategory.Building)
			{
				int num2 = Mathf.RoundToInt((float)dinfo.Amount * this.def.explosionBuildingDamageFactor);
				damageDef = this.def;
				amount = num2;
				angle = dinfo.Angle;
				instigator = explosion.instigator;
				weapon = dinfo.Weapon;
				dinfo = new DamageInfo(damageDef, amount, angle, instigator, null, weapon, DamageInfo.SourceCategory.ThingOrUnknown);
			}
			BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
			if (t is Pawn)
			{
				battleLogEntry_ExplosionImpact = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, this.def);
				Find.BattleLog.Add(battleLogEntry_ExplosionImpact);
			}
			t.TakeDamage(dinfo).InsertIntoLog(battleLogEntry_ExplosionImpact);
		}

		public IEnumerable<IntVec3> ExplosionCellsToHit(Explosion explosion)
		{
			return this.ExplosionCellsToHit(explosion.Position, explosion.Map, explosion.radius);
		}

		public virtual IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius)
		{
			DamageWorker.openCells.Clear();
			DamageWorker.adjWallCells.Clear();
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					if (GenSight.LineOfSight(center, intVec, map, true, null, 0, 0))
					{
						DamageWorker.openCells.Add(intVec);
					}
				}
			}
			for (int j = 0; j < DamageWorker.openCells.Count; j++)
			{
				IntVec3 intVec2 = DamageWorker.openCells[j];
				if (intVec2.Walkable(map))
				{
					for (int k = 0; k < 4; k++)
					{
						IntVec3 intVec3 = intVec2 + GenAdj.CardinalDirections[k];
						if (intVec3.InHorDistOf(center, radius))
						{
							if (intVec3.InBounds(map))
							{
								if (!intVec3.Standable(map))
								{
									if (intVec3.GetEdifice(map) != null)
									{
										if (!DamageWorker.openCells.Contains(intVec3) && DamageWorker.adjWallCells.Contains(intVec3))
										{
											DamageWorker.adjWallCells.Add(intVec3);
										}
									}
								}
							}
						}
					}
				}
			}
			return DamageWorker.openCells.Concat(DamageWorker.adjWallCells);
		}
	}
}
