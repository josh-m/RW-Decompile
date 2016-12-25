using System;
using UnityEngine;

namespace Verse
{
	public static class Altitudes
	{
		private const int NumAltitudeLayers = 29;

		private const float LayerSpacing = 0.5f;

		public const float AltInc = 0.05f;

		private static readonly float[] Alts;

		public static readonly Vector3 AltIncVect;

		static Altitudes()
		{
			Altitudes.Alts = new float[29];
			Altitudes.AltIncVect = new Vector3(0f, 0.05f, 0f);
			if (Enum.GetValues(typeof(AltitudeLayer)).Length != 29)
			{
				Log.Message("Altitudes.NumAltitudeLayers should be " + Enum.GetValues(typeof(AltitudeLayer)).Length);
			}
			for (int i = 0; i < 29; i++)
			{
				Altitudes.Alts[i] = (float)i * 0.5f;
			}
		}

		public static float AltitudeFor(AltitudeLayer alt)
		{
			return Altitudes.Alts[(int)alt];
		}
	}
}
