using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Verb_LaunchProjectile : Verb
	{
		protected override bool TryCastShot()
		{
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			ShootLine shootLine;
			bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
			if (this.verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			Vector3 drawPos = this.caster.DrawPos;
			Projectile projectile = (Projectile)GenSpawn.Spawn(this.verbProps.projectileDef, shootLine.Source, this.caster.Map);
			projectile.FreeIntercept = (this.canFreeInterceptNow && !projectile.def.projectile.flyOverhead);
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
							projectile.ThingToNeverIntercept = this.currentTarget.Thing;
						}
						if (!projectile.def.projectile.flyOverhead)
						{
							projectile.InterceptWalls = true;
						}
						projectile.Launch(this.caster, drawPos, c, this.ownerEquipment);
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
					projectile.ThingToNeverIntercept = this.currentTarget.Thing;
				}
				if (!projectile.def.projectile.flyOverhead)
				{
					projectile.InterceptWalls = true;
				}
				projectile.Launch(this.caster, drawPos, shootLine.Dest, this.ownerEquipment);
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
					if (!projectile.def.projectile.flyOverhead)
					{
						projectile.InterceptWalls = true;
					}
					projectile.Launch(this.caster, drawPos, randomCoverToMissInto, this.ownerEquipment);
					return true;
				}
			}
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToHit", -1f);
			}
			if (!projectile.def.projectile.flyOverhead)
			{
				projectile.InterceptWalls = (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full);
			}
			if (this.currentTarget.Thing != null)
			{
				projectile.Launch(this.caster, drawPos, this.currentTarget, this.ownerEquipment);
			}
			else
			{
				projectile.Launch(this.caster, drawPos, shootLine.Dest, this.ownerEquipment);
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget()
		{
			return this.verbProps.projectileDef.projectile.explosionRadius;
		}
	}
}
