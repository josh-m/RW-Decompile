using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldDebugMatsSpectrum
	{
		public const int MaterialCount = 100;

		private const float Opacity = 0.25f;

		private static readonly Material[] spectrumMats;

		private static readonly Color[] DebugSpectrum;

		static WorldDebugMatsSpectrum()
		{
			WorldDebugMatsSpectrum.spectrumMats = new Material[100];
			WorldDebugMatsSpectrum.DebugSpectrum = DebugMatsSpectrum.DebugSpectrum;
			for (int i = 0; i < 100; i++)
			{
				WorldDebugMatsSpectrum.spectrumMats[i] = MatsFromSpectrum.Get(WorldDebugMatsSpectrum.DebugSpectrum, (float)i / 100f, ShaderDatabase.WorldOverlayTransparent);
			}
		}

		public static Material Mat(int ind)
		{
			ind = Mathf.Clamp(ind, 0, 99);
			return WorldDebugMatsSpectrum.spectrumMats[ind];
		}
	}
}
