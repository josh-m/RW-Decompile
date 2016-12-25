using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MatLoader
	{
		private static Dictionary<Material, Material> dict = new Dictionary<Material, Material>();

		public static Material LoadMat(string matPath)
		{
			Material material = (Material)Resources.Load("Materials/" + matPath, typeof(Material));
			if (material == null)
			{
				Log.Warning("Could not load material " + matPath);
			}
			Material material2;
			if (!MatLoader.dict.TryGetValue(material, out material2))
			{
				material2 = new Material(material);
				MatLoader.dict.Add(material, material2);
			}
			return material2;
		}
	}
}
