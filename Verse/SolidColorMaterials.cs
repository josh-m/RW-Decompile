using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class SolidColorMaterials
	{
		private static Dictionary<Color, Material> simpleColorMats = new Dictionary<Color, Material>();

		public static int SimpleColorMatCount
		{
			get
			{
				return SolidColorMaterials.simpleColorMats.Count;
			}
		}

		public static Material SimpleSolidColorMaterial(Color col)
		{
			Material material;
			if (!SolidColorMaterials.simpleColorMats.TryGetValue(col, out material))
			{
				material = SolidColorMaterials.NewSolidColorMaterial(col, ShaderDatabase.SolidColor);
				SolidColorMaterials.simpleColorMats.Add(col, material);
			}
			return material;
		}

		public static Material NewSolidColorMaterial(Color col, Shader shader)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to create a material from a different thread.");
				return null;
			}
			return new Material(shader)
			{
				color = col,
				name = string.Concat(new object[]
				{
					"SolidColorMat-",
					shader.name,
					"-",
					col
				})
			};
		}

		public static Texture2D NewSolidColorTexture(float r, float g, float b, float a)
		{
			return SolidColorMaterials.NewSolidColorTexture(new Color(r, g, b, a));
		}

		public static Texture2D NewSolidColorTexture(Color color)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to create a texture from a different thread.");
				return null;
			}
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.name = "SolidColorTex-" + color;
			texture2D.SetPixel(0, 0, color);
			texture2D.Apply();
			return texture2D;
		}
	}
}
