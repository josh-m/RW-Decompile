using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompPowerPlantSolar : CompPowerPlant
	{
		private const float FullSunPower = 1700f;

		private const float NightPower = 0f;

		private static readonly Vector2 BarSize = new Vector2(2.3f, 0.14f);

		private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

		private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

		protected override float DesiredPowerOutput
		{
			get
			{
				return Mathf.Lerp(0f, 1700f, this.parent.Map.skyManager.CurSkyGlow) * this.RoofedPowerOutputFactor;
			}
		}

		private float RoofedPowerOutputFactor
		{
			get
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 current in this.parent.OccupiedRect())
				{
					num++;
					if (this.parent.Map.roofGrid.Roofed(current))
					{
						num2++;
					}
				}
				return (float)(num - num2) / (float)num;
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = this.parent.DrawPos + Vector3.up * 0.1f;
			r.size = CompPowerPlantSolar.BarSize;
			r.fillPercent = base.PowerOutput / 1700f;
			r.filledMat = CompPowerPlantSolar.PowerPlantSolarBarFilledMat;
			r.unfilledMat = CompPowerPlantSolar.PowerPlantSolarBarUnfilledMat;
			r.margin = 0.15f;
			Rot4 rotation = this.parent.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
		}
	}
}
