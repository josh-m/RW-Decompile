using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse.Noise;

namespace Verse
{
	public static class WindManager
	{
		private static List<Material> plantMaterials = new List<Material>();

		private static float cachedWindSpeed;

		private static ModuleBase windNoise = null;

		private static float plantSwayHead = 0f;

		public static float WindSpeed
		{
			get
			{
				return WindManager.cachedWindSpeed;
			}
		}

		public static void Reinit()
		{
			WindManager.windNoise = null;
		}

		public static void WindManagerTick()
		{
			WindManager.cachedWindSpeed = WindManager.BaseWindSpeedAt(Find.TickManager.TicksAbs) * Find.WeatherManager.CurWindSpeedFactor;
			if (Prefs.PlantWindSway)
			{
				WindManager.plantSwayHead += Mathf.Min(WindManager.WindSpeed, 1f);
			}
			else
			{
				WindManager.plantSwayHead = 0f;
			}
			for (int i = 0; i < WindManager.plantMaterials.Count; i++)
			{
				WindManager.plantMaterials[i].SetFloat("_SwayHead", WindManager.plantSwayHead);
			}
		}

		public static void Notify_PlantMaterialCreated(Material newMat)
		{
			WindManager.plantMaterials.Add(newMat);
		}

		private static float BaseWindSpeedAt(int ticksAbs)
		{
			if (WindManager.windNoise == null)
			{
				int seed = Find.Map.WorldCoords.x + Find.Map.WorldCoords.x * 10000 ^ Find.World.info.Seed;
				WindManager.windNoise = new Perlin(3.9999998989515007E-05, 2.0, 0.5, 4, seed, QualityMode.Medium);
				WindManager.windNoise = new ScaleBias(1.5, 0.5, WindManager.windNoise);
				WindManager.windNoise = new Clamp(0.039999999105930328, 2.0, WindManager.windNoise);
			}
			return (float)WindManager.windNoise.GetValue((double)ticksAbs, 0.0, 0.0);
		}

		public static string DebugString()
		{
			return string.Concat(new object[]
			{
				"WindSpeed: ",
				WindManager.WindSpeed,
				"\nplantSwayHead: ",
				WindManager.plantSwayHead
			});
		}

		public static void LogWindSpeeds()
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
					WindManager.BaseWindSpeedAt(Find.TickManager.TicksAbs + 2500 * i).ToString("F2")
				}));
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
