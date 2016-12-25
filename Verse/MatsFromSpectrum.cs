using System;
using UnityEngine;

namespace Verse
{
	public static class MatsFromSpectrum
	{
		public static Material Get(Color[] spectrum, float val)
		{
			Color col = ColorsFromSpectrum.Get(spectrum, val);
			return SolidColorMaterials.NewSolidColorMaterial(col, ShaderDatabase.MetaOverlay);
		}
	}
}
