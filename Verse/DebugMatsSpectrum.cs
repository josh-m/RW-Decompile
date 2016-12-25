using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class DebugMatsSpectrum
	{
		public const int MaterialCount = 100;

		private const float Opacity = 0.25f;

		private static readonly Material[] spectrumMats;

		public static readonly Color[] DebugSpectrum;

		static DebugMatsSpectrum()
		{
			DebugMatsSpectrum.spectrumMats = new Material[100];
			DebugMatsSpectrum.DebugSpectrum = new Color[]
			{
				new Color(0.75f, 0f, 0f, 0.25f),
				new Color(0.5f, 0.3f, 0f, 0.25f),
				new Color(0f, 1f, 0f, 0.25f),
				new Color(0f, 0f, 1f, 0.25f),
				new Color(0.7f, 0f, 1f, 0.25f)
			};
			for (int i = 0; i < 100; i++)
			{
				DebugMatsSpectrum.spectrumMats[i] = MatsFromSpectrum.Get(DebugMatsSpectrum.DebugSpectrum, (float)i / 100f);
			}
		}

		public static Material Mat(int ind)
		{
			if (ind >= 100)
			{
				ind = 99;
			}
			if (ind < 0)
			{
				ind = 0;
			}
			return DebugMatsSpectrum.spectrumMats[ind];
		}
	}
}
