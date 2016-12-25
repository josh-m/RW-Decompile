using System;
using UnityEngine;

namespace Verse
{
	public static class ColorsFromSpectrum
	{
		public static Color Get(Color[] spectrum, float val)
		{
			val = Mathf.Clamp01(val);
			val *= (float)(spectrum.Length - 1);
			int num = 0;
			while (val > 1f)
			{
				val -= 1f;
				num++;
				if (num > spectrum.Length - 1)
				{
					Log.Error("Hit spectrum limit.");
					num--;
					break;
				}
			}
			return Color.Lerp(spectrum[num], spectrum[num + 1], val);
		}
	}
}
