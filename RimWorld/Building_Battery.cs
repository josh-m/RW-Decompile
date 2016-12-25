using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_Battery : Building
	{
		private const float MinEnergyToExplode = 500f;

		private const float EnergyToLoseWhenExplode = 400f;

		private const float ExplodeChancePerDamage = 0.05f;

		private int ticksToExplode;

		private Sustainer wickSustainer;

		private static readonly Vector2 BarSize = new Vector2(1.3f, 0.4f);

		private static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));

		private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksToExplode, "ticksToExplode", 0, false);
		}

		public override void Draw()
		{
			base.Draw();
			CompPowerBattery comp = base.GetComp<CompPowerBattery>();
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = this.DrawPos + Vector3.up * 0.1f;
			r.size = Building_Battery.BarSize;
			r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
			r.filledMat = Building_Battery.BatteryBarFilledMat;
			r.unfilledMat = Building_Battery.BatteryBarUnfilledMat;
			r.margin = 0.15f;
			Rot4 rotation = base.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
			if (this.ticksToExplode > 0 && base.Spawned)
			{
				base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (this.ticksToExplode > 0)
			{
				if (this.wickSustainer == null)
				{
					this.StartWickSustainer();
				}
				else
				{
					this.wickSustainer.Maintain();
				}
				this.ticksToExplode--;
				if (this.ticksToExplode == 0)
				{
					IntVec3 randomCell = this.OccupiedRect().RandomCell;
					float radius = Rand.Range(0.5f, 1f) * 3f;
					GenExplosion.DoExplosion(randomCell, base.Map, radius, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
					base.GetComp<CompPowerBattery>().DrawPower(400f);
				}
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (!base.Destroyed && this.ticksToExplode == 0 && dinfo.Def == DamageDefOf.Flame && Rand.Value < 0.05f && base.GetComp<CompPowerBattery>().StoredEnergy > 500f)
			{
				this.ticksToExplode = Rand.Range(70, 150);
				this.StartWickSustainer();
			}
		}

		private void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			this.wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
		}
	}
}
