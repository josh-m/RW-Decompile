using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse.Noise;

namespace Verse
{
	public class WindManager
	{
		private Map map;

		private static List<Material> plantMaterials = new List<Material>();

		private float cachedWindSpeed;

		private ModuleBase windNoise;

		private float plantSwayHead;

		public float WindSpeed
		{
			get
			{
				return this.cachedWindSpeed;
			}
		}

		public WindManager(Map map)
		{
			this.map = map;
		}

		public void WindManagerTick()
		{
			this.cachedWindSpeed = this.BaseWindSpeedAt(Find.TickManager.TicksAbs) * this.map.weatherManager.CurWindSpeedFactor;
			if (Prefs.PlantWindSway)
			{
				this.plantSwayHead += Mathf.Min(this.WindSpeed, 1f);
			}
			else
			{
				this.plantSwayHead = 0f;
			}
			if (Find.VisibleMap == this.map)
			{
				for (int i = 0; i < WindManager.plantMaterials.Count; i++)
				{
					WindManager.plantMaterials[i].SetFloat("_SwayHead", this.plantSwayHead);
				}
			}
		}

		public static void Notify_PlantMaterialCreated(Material newMat)
		{
			WindManager.plantMaterials.Add(newMat);
		}

		private float BaseWindSpeedAt(int ticksAbs)
		{
			if (this.windNoise == null)
			{
				int seed = Gen.HashCombineInt(this.map.Tile, 122049541) ^ Find.World.info.Seed;
				this.windNoise = new Perlin(3.9999998989515007E-05, 2.0, 0.5, 4, seed, QualityMode.Medium);
				this.windNoise = new ScaleBias(1.5, 0.5, this.windNoise);
				this.windNoise = new Clamp(0.039999999105930328, 2.0, this.windNoise);
			}
			return (float)this.windNoise.GetValue((double)ticksAbs, 0.0, 0.0);
		}

		public string DebugString()
		{
			return string.Concat(new object[]
			{
				"WindSpeed: ",
				this.WindSpeed,
				"\nplantSwayHead: ",
				this.plantSwayHead
			});
		}

		public void LogWindSpeeds()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Upcoming wind speeds:");
			for (int i = 0; i < 72; i++)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"Hour ",
					i,
					" - ",
					this.BaseWindSpeedAt(Find.TickManager.TicksAbs + 2500 * i).ToString("F2")
				}));
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
