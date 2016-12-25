using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldMaterials
	{
		private const float TempRange = 50f;

		private const float ElevationMax = 5000f;

		private const float RainfallMax = 5000f;

		public static Material matHill;

		public static Material matMountain;

		private static int NumMatsPerMode;

		public static Material OverlayModeMatOcean;

		private static Material[] matsFertility;

		private static readonly Color[] FertilitySpectrum;

		private static Material[] matsTemperature;

		private static readonly Color[] TemperatureSpectrum;

		private static Material[] matsElevation;

		private static readonly Color[] ElevationSpectrum;

		private static Material[] matsRainfall;

		private static readonly Color[] RainfallSpectrum;

		static WorldMaterials()
		{
			WorldMaterials.matHill = MaterialPool.MatFrom("World/Hill");
			WorldMaterials.matMountain = MaterialPool.MatFrom("World/Mountain");
			WorldMaterials.NumMatsPerMode = 50;
			WorldMaterials.OverlayModeMatOcean = SolidColorMaterials.NewSolidColorMaterial(new Color(0.09f, 0.18f, 0.2f), ShaderDatabase.Transparent);
			WorldMaterials.FertilitySpectrum = new Color[]
			{
				new Color(0f, 1f, 0f, 0f),
				new Color(0f, 1f, 0f, 0.5f)
			};
			WorldMaterials.TemperatureSpectrum = new Color[]
			{
				new Color(1f, 1f, 1f),
				new Color(0f, 0f, 1f),
				new Color(0.25f, 0.25f, 1f),
				new Color(0.6f, 0.6f, 1f),
				new Color(0.5f, 0.5f, 0.5f),
				new Color(0.5f, 0.3f, 0f),
				new Color(1f, 0.6f, 0.18f),
				new Color(1f, 0f, 0f)
			};
			WorldMaterials.ElevationSpectrum = new Color[]
			{
				new Color(0.224f, 0.18f, 0.15f),
				new Color(0.447f, 0.369f, 0.298f),
				new Color(0.6f, 0.6f, 0.6f),
				new Color(1f, 1f, 1f)
			};
			WorldMaterials.RainfallSpectrum = new Color[]
			{
				new Color(0.9f, 0.9f, 0.9f),
				GenColor.FromBytes(190, 190, 190, 255),
				new Color(0.58f, 0.58f, 0.58f),
				GenColor.FromBytes(196, 112, 110, 255),
				GenColor.FromBytes(200, 179, 150, 255),
				GenColor.FromBytes(255, 199, 117, 255),
				GenColor.FromBytes(255, 255, 84, 255),
				GenColor.FromBytes(145, 255, 253, 255),
				GenColor.FromBytes(0, 255, 0, 255),
				GenColor.FromBytes(63, 198, 55, 255),
				GenColor.FromBytes(13, 150, 5, 255),
				GenColor.FromBytes(5, 112, 94, 255)
			};
			WorldMaterials.GenerateMats(ref WorldMaterials.matsFertility, WorldMaterials.FertilitySpectrum, WorldMaterials.NumMatsPerMode);
			WorldMaterials.GenerateMats(ref WorldMaterials.matsTemperature, WorldMaterials.TemperatureSpectrum, WorldMaterials.NumMatsPerMode);
			WorldMaterials.GenerateMats(ref WorldMaterials.matsElevation, WorldMaterials.ElevationSpectrum, WorldMaterials.NumMatsPerMode);
			WorldMaterials.GenerateMats(ref WorldMaterials.matsRainfall, WorldMaterials.RainfallSpectrum, WorldMaterials.NumMatsPerMode);
		}

		private static void GenerateMats(ref Material[] mats, Color[] colorSpectrum, int numMats)
		{
			mats = new Material[numMats];
			for (int i = 0; i < numMats; i++)
			{
				mats[i] = MatsFromSpectrum.Get(colorSpectrum, (float)i / (float)numMats);
			}
		}

		public static Material MatForFertilityOverlay(float fert)
		{
			int value = Mathf.FloorToInt(fert * (float)WorldMaterials.NumMatsPerMode);
			return WorldMaterials.matsFertility[Mathf.Clamp(value, 0, WorldMaterials.NumMatsPerMode - 1)];
		}

		public static Material MatForTemperature(float temp)
		{
			float num = Mathf.InverseLerp(-50f, 50f, temp);
			int value = Mathf.FloorToInt(num * (float)WorldMaterials.NumMatsPerMode);
			return WorldMaterials.matsTemperature[Mathf.Clamp(value, 0, WorldMaterials.NumMatsPerMode - 1)];
		}

		public static Material MatForElevation(float elev)
		{
			float num = Mathf.InverseLerp(0f, 5000f, elev);
			int value = Mathf.FloorToInt(num * (float)WorldMaterials.NumMatsPerMode);
			return WorldMaterials.matsElevation[Mathf.Clamp(value, 0, WorldMaterials.NumMatsPerMode - 1)];
		}

		public static Material MatForRainfallOverlay(float rain)
		{
			float num = Mathf.InverseLerp(0f, 5000f, rain);
			int value = Mathf.FloorToInt(num * (float)WorldMaterials.NumMatsPerMode);
			return WorldMaterials.matsRainfall[Mathf.Clamp(value, 0, WorldMaterials.NumMatsPerMode - 1)];
		}
	}
}
