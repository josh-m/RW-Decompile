using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherEvent_LightningStrike : WeatherEvent_LightningFlash
	{
		private IntVec3 strikeLoc = IntVec3.Invalid;

		private Mesh boltMesh;

		private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

		public WeatherEvent_LightningStrike()
		{
		}

		public WeatherEvent_LightningStrike(IntVec3 forcedStrikeLoc) : this()
		{
			this.strikeLoc = forcedStrikeLoc;
		}

		public override void FireEvent()
		{
			base.FireEvent();
			if (!this.strikeLoc.IsValid)
			{
				this.strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable() && !Find.RoofGrid.Roofed(sq), 1000);
			}
			this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
			GenExplosion.DoExplosion(this.strikeLoc, 1.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			Vector3 loc = this.strikeLoc.ToVector3Shifted();
			for (int i = 0; i < 4; i++)
			{
				MoteMaker.ThrowSmoke(loc, 1.5f);
				MoteMaker.ThrowMicroSparks(loc);
				MoteMaker.ThrowLightningGlow(loc, 1.5f);
			}
			SoundInfo info = SoundInfo.InWorld(this.strikeLoc, MaintenanceType.None);
			SoundDefOf.Thunder_OnMap.PlayOneShot(info);
		}

		public override void WeatherEventDraw()
		{
			Graphics.DrawMesh(this.boltMesh, this.strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(WeatherEvent_LightningStrike.LightningMat, base.LightningBrightness), 0);
		}
	}
}
