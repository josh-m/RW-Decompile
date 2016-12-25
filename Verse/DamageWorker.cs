using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class DamageWorker
	{
		private const float ExplosionCamShakeMultiplier = 4f;

		public DamageDef def;

		private static List<Thing> thingsToAffect = new List<Thing>();

		private static List<IntVec3> openCells = new List<IntVec3>();

		private static List<IntVec3> adjWallCells = new List<IntVec3>();

		public virtual float Apply(DamageInfo dinfo, Thing victim)
		{
			float result = 0f;
			if (victim.def.useHitPoints && dinfo.Def.harmsHealth)
			{
				result = (float)Mathf.Min(victim.HitPoints, dinfo.Amount);
				victim.HitPoints -= dinfo.Amount;
				if (victim.HitPoints <= 0)
				{
					victim.HitPoints = 0;
					victim.Destroy(DestroyMode.Kill);
				}
			}
			ImpactSoundUtility.PlayImpactSound(victim, dinfo.Def.impactSoundType);
			return result;
		}

		public virtual void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
		{
			if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
			{
				GenTemperature.PushHeat(explosion.position, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
			}
			MoteMaker.MakeStaticMote(explosion.position, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
			float magnitude = (explosion.position.ToVector3() - Find.Camera.transform.position).magnitude;
			Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
			this.ExplosionVisualEffectCenter(explosion);
		}

		protected virtual void ExplosionVisualEffectCenter(Explosion explosion)
		{
			for (int i = 0; i < 4; i++)
			{
				MoteMaker.ThrowSmoke(explosion.position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.radius * 0.6f);
			}
			if (this.def.explosionInteriorMote != null)
			{
				int num = Mathf.RoundToInt(3.14159274f * explosion.radius * explosion.radius / 6f);
				for (int j = 0; j < num; j++)
				{
					MoteMaker.ThrowExplosionInteriorMote(explosion.position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), this.def.explosionInteriorMote);
				}
			}
		}

		public virtual void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, bool canThrowMotes)
		{
			if (!c.InBounds())
			{
				return;
			}
			if (this.def.explosionCellMote != null && canThrowMotes)
			{
				float t = Mathf.Clamp01((explosion.position - c).LengthHorizontal / explosion.radius);
				Color color = Color.Lerp(this.def.explosionColorCenter, this.def.explosionColorEdge, t);
				MoteMaker.ThrowExplosionCell(c, this.def.explosionCellMote, color);
			}
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
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
					this.ExplosionDamageThing(explosion, DamageWorker.thingsToAffect[j], damagedThings);
				}
			}
			if (this.def.explosionSnowMeltAmount > 0.0001f)
			{
				float lengthHorizontal = (c - explosion.position).LengthHorizontal;
				float num2 = 1f - lengthHorizontal / explosion.radius;
				if (num2 > 0f)
				{
					Find.SnowGrid.AddDepth(c, -num2 * this.def.explosionSnowMeltAmount);
				}
			}
		}

		protected virtual void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings)
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
			float direction;
			if (t.Position == explosion.position)
			{
				direction = (float)Rand.RangeInclusive(0, 359);
			}
			else
			{
				direction = (t.Position - explosion.position).AngleFlat;
			}
			DamageInfo dinfo = new DamageInfo(this.def, explosion.damAmount, explosion.instigator, direction, null, explosion.source);
			if (this.def.explosionAffectOutsidePartsOnly)
			{
				dinfo.SetPart(new BodyPartDamageInfo(null, new BodyPartDepth?(BodyPartDepth.Outside)));
			}
			else
			{
				dinfo.SetPart(new BodyPartDamageInfo(null, null));
			}
			if (t.def.category == ThingCategory.Building)
			{
				int amount = Mathf.RoundToInt((float)dinfo.Amount * this.def.explosionBuildingDamageFactor);
				dinfo = new DamageInfo(this.def, amount, explosion.instigator, dinfo.Angle, null, null);
			}
			t.TakeDamage(dinfo);
		}

		public IEnumerable<IntVec3> ExplosionCellsToHit(Explosion explosion)
		{
			return this.ExplosionCellsToHit(explosion.position, explosion.radius);
		}

		public virtual IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, float radius)
		{
			DamageWorker.openCells.Clear();
			DamageWorker.adjWallCells.Clear();
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (intVec.InBounds())
				{
					if (GenSight.LineOfSight(center, intVec, true))
					{
						DamageWorker.openCells.Add(intVec);
					}
				}
			}
			for (int j = 0; j < DamageWorker.openCells.Count; j++)
			{
				IntVec3 intVec2 = DamageWorker.openCells[j];
				if (intVec2.Walkable())
				{
					for (int k = 0; k < 4; k++)
					{
						IntVec3 intVec3 = intVec2 + GenAdj.CardinalDirections[k];
						if (intVec3.InHorDistOf(center, radius))
						{
							if (intVec3.InBounds())
							{
								if (!intVec3.Standable())
								{
									if (intVec3.GetEdifice() != null)
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
