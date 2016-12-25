using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class DebugSolidColorMats
	{
		private static Dictionary<Color, Material> colorMatDict = new Dictionary<Color, Material>();

		public static Material MaterialOf(Color col)
		{
			Material material;
			if (DebugSolidColorMats.colorMatDict.TryGetValue(col, out material))
			{
				return material;
			}
			material = SolidColorMaterials.SimpleSolidColorMaterial(col);
			DebugSolidColorMats.colorMatDict.Add(col, material);
			return material;
		}
	}
}
