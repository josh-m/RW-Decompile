using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Verb_LaunchProjectile : Verb
	{
		public virtual ThingDef Projectile
		{
			get
			{
				if (this.ownerEquipment != null)
				{
					CompChangeableProjectile comp = this.ownerEquipment.GetComp<CompChangeableProjectile>();
					if (comp != null && comp.Loaded)
					{
						return comp.Projectile;
					}
				}
				return this.verbProps.defaultProjectile;
			}
		}

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster, (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing, (this.ownerEquipment == null) ? null : this.ownerEquipment.def, this.Projectile, this.ShotsPerBurst > 1));
		}

		protected override bool TryCastShot()
		{
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			ThingDef projectile = this.Projectile;
			if (projectile == null)
			{
				return false;
			}
			ShootLine shootLine;
			bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
			if (this.verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (this.ownerEquipment != null)
			{
				CompChangeableProjectile comp = this.ownerEquipment.GetComp<CompChangeableProjectile>();
				if (comp != null)
				{
					comp.Notify_ProjectileLaunched();
				}
			}
			Thing launcher = this.caster;
			Thing equipment = this.ownerEquipment;
			CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
			if (compMannable != null && compMannable.ManningPawn != null)
			{
				launcher = compMannable.ManningPawn;
				equipment = this.caster;
			}
			Vector3 drawPos = this.caster.DrawPos;
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map);
			projectile2.FreeIntercept = (this.canFreeInterceptNow && !projectile2.def.projectile.flyOverhead);
			if (this.verbProps.forcedMissRadius > 0.5f)
			{
				float num = (float)(this.currentTarget.Cell - this.caster.Position).LengthHorizontalSquared;
				float num2;
				if (num < 9f)
				{
					num2 = 0f;
				}
				else if (num < 25f)
				{
					num2 = this.verbProps.forcedMissRadius * 0.5f;
				}
				else if (num < 49f)
				{
					num2 = this.verbProps.forcedMissRadius * 0.8f;
				}
				else
				{
					num2 = this.verbProps.forcedMissRadius * 1f;
				}
				if (num2 > 0.5f)
				{
					int max = GenRadial.NumCellsInRadius(this.verbProps.forcedMissRadius);
					int num3 = Rand.Range(0, max);
					if (num3 > 0)
					{
						if (DebugViewSettings.drawShooting)
						{
							MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToForRad", -1f);
						}
						IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num3];
						if (this.currentTarget.HasThing)
						{
							projectile2.ThingToNeverIntercept = this.currentTarget.Thing;
						}
						if (!projectile2.def.projectile.flyOverhead)
						{
							projectile2.InterceptWalls = true;
						}
						projectile2.Launch(launcher, drawPos, c, equipment, this.currentTarget.Thing);
						return true;
					}
				}
			}
			ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
			if (Rand.Value > shotReport.ChanceToNotGoWild_IgnoringPosture)
			{
				if (DebugViewSettings.drawShooting)
				{
					MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToWild", -1f);
				}
				shootLine.ChangeDestToMissWild();
				if (this.currentTarget.HasThing)
				{
					projectile2.ThingToNeverIntercept = this.currentTarget.Thing;
				}
				if (!projectile2.def.projectile.flyOverhead)
				{
					projectile2.InterceptWalls = true;
				}
				projectile2.Launch(launcher, drawPos, shootLine.Dest, equipment, this.currentTarget.Thing);
				return true;
			}
			if (Rand.Value > shotReport.ChanceToNotHitCover)
			{
				if (DebugViewSettings.drawShooting)
				{
					MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToCover", -1f);
				}
				if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
				{
					Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
					if (!projectile2.def.projectile.flyOverhead)
					{
						projectile2.InterceptWalls = true;
					}
					projectile2.Launch(launcher, drawPos, randomCoverToMissInto, equipment, this.currentTarget.Thing);
					return true;
				}
			}
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToHit", -1f);
			}
			if (!projectile2.def.projectile.flyOverhead)
			{
				projectile2.InterceptWalls = (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full);
			}
			if (this.currentTarget.Thing != null)
			{
				projectile2.Launch(launcher, drawPos, this.currentTarget, equipment, this.currentTarget.Thing);
			}
			else
			{
				projectile2.Launch(launcher, drawPos, shootLine.Dest, equipment, this.currentTarget.Thing);
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = true;
			ThingDef projectile = this.Projectile;
			if (projectile == null)
			{
				return 0f;
			}
			return projectile.projectile.explosionRadius;
		}
	}
}
