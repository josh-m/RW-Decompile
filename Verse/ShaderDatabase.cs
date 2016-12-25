using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ShaderDatabase
	{
		public static readonly Shader Cutout = ShaderDatabase.LoadShader("Map/Cutout");

		public static readonly Shader CutoutPlant = ShaderDatabase.LoadShader("Map/CutoutPlant");

		public static readonly Shader CutoutComplex = ShaderDatabase.LoadShader("Map/CutoutComplex");

		public static readonly Shader CutoutSkin = ShaderDatabase.LoadShader("Map/CutoutSkin");

		public static readonly Shader CutoutFlying = ShaderDatabase.LoadShader("Map/CutoutFlying");

		public static readonly Shader Transparent = ShaderDatabase.LoadShader("Map/Transparent");

		public static readonly Shader TransparentPostLight = ShaderDatabase.LoadShader("Map/TransparentPostLight");

		public static readonly Shader Mote = ShaderDatabase.LoadShader("Map/Mote");

		public static readonly Shader MoteGlow = ShaderDatabase.LoadShader("Map/MoteGlow");

		public static readonly Shader TerrainHard = ShaderDatabase.LoadShader("Map/TerrainHard");

		public static readonly Shader TerrainFade = ShaderDatabase.LoadShader("Map/TerrainFade");

		public static readonly Shader TerrainFadeRough = ShaderDatabase.LoadShader("Map/TerrainFadeRough");

		public static readonly Shader WorldTerrain = ShaderDatabase.LoadShader("World/WorldTerrain");

		public static readonly Shader WorldOcean = ShaderDatabase.LoadShader("World/WorldOcean");

		public static readonly Shader WorldOverlayCutout = ShaderDatabase.LoadShader("World/WorldOverlayCutout");

		public static readonly Shader WorldOverlayTransparent = ShaderDatabase.LoadShader("World/WorldOverlayTransparent");

		public static readonly Shader WorldOverlayTransparentLit = ShaderDatabase.LoadShader("World/WorldOverlayTransparentLit");

		public static readonly Shader WorldOverlayAdditive = ShaderDatabase.LoadShader("World/WorldOverlayAdditive");

		public static readonly Shader MetaOverlay = ShaderDatabase.LoadShader("Map/MetaOverlay");

		public static readonly Shader SolidColor = ShaderDatabase.LoadShader("Map/SolidColor");

		public static Shader DefaultShader
		{
			get
			{
				return ShaderDatabase.Cutout;
			}
		}

		public static Shader ShaderFromType(ShaderType sType)
		{
			switch (sType)
			{
			case ShaderType.Cutout:
				return ShaderDatabase.Cutout;
			case ShaderType.CutoutFlying:
				return ShaderDatabase.CutoutFlying;
			case ShaderType.CutoutPlant:
				return ShaderDatabase.CutoutPlant;
			case ShaderType.CutoutComplex:
				return ShaderDatabase.CutoutComplex;
			case ShaderType.CutoutSkin:
				return ShaderDatabase.CutoutSkin;
			case ShaderType.Transparent:
				return ShaderDatabase.Transparent;
			case ShaderType.TransparentPostLight:
				return ShaderDatabase.TransparentPostLight;
			case ShaderType.MetaOverlay:
				return ShaderDatabase.MetaOverlay;
			case ShaderType.Mote:
				return ShaderDatabase.Mote;
			case ShaderType.MoteGlow:
				return ShaderDatabase.MoteGlow;
			default:
				Log.ErrorOnce("Unknown ShaderType " + sType, 2766893);
				return ShaderDatabase.DefaultShader;
			}
		}

		private static Shader LoadShader(string shaderPath)
		{
			Shader shader = (Shader)Resources.Load("Materials/" + shaderPath, typeof(Shader));
			if (shader == null)
			{
				Log.Warning("Could not load shader " + shaderPath);
				return ShaderDatabase.DefaultShader;
			}
			return shader;
		}
	}
}
