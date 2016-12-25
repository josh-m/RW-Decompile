using System;
using UnityEngine;

namespace Verse
{
	public static class Pulser
	{
		public static float PulseBrightness(float frequency, float amplitude)
		{
			return Pulser.PulseBrightness(frequency, amplitude, Time.realtimeSinceStartup);
		}

		public static float PulseBrightness(float frequency, float amplitude, float time)
		{
			float num = time * 6.28318548f;
			num *= frequency;
			float t = (1f - Mathf.Cos(num)) * 0.5f;
			return Mathf.Lerp(1f - amplitude, 1f, t);
		}
	}
}
