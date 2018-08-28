using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PlantFallColors
	{
		[TweakValue("Graphics", 0f, 1f)]
		private static float FallColorBegin = 0.55f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallColorEnd = 0.45f;

		[TweakValue("Graphics", 0f, 30f)]
		private static float FallSlopeComponent = 15f;

		[TweakValue("Graphics", 0f, 100f)]
		private static bool FallIntensityOverride;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallIntensity;

		[TweakValue("Graphics", 0f, 100f)]
		private static bool FallGlobalControls;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallSrcR = 0.3803f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallSrcG = 0.4352f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallSrcB = 0.1451f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallDstR = 0.4392f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallDstG = 0.3254f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallDstB = 0.1765f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallRangeBegin = 0.02f;

		[TweakValue("Graphics", 0f, 1f)]
		private static float FallRangeEnd = 0.1f;

		public static float GetFallColorFactor(float latitude, int dayOfYear)
		{
			float a = GenCelestial.AverageGlow(latitude, dayOfYear);
			float b = GenCelestial.AverageGlow(latitude, dayOfYear + 1);
			float x = Mathf.LerpUnclamped(a, b, PlantFallColors.FallSlopeComponent);
			return GenMath.LerpDoubleClamped(PlantFallColors.FallColorBegin, PlantFallColors.FallColorEnd, 0f, 1f, x);
		}

		public static void SetFallShaderGlobals(Map map)
		{
			if (PlantFallColors.FallIntensityOverride)
			{
				Shader.SetGlobalFloat(ShaderPropertyIDs.FallIntensity, PlantFallColors.FallIntensity);
			}
			else
			{
				Vector2 vector = Find.WorldGrid.LongLatOf(map.Tile);
				Shader.SetGlobalFloat(ShaderPropertyIDs.FallIntensity, PlantFallColors.GetFallColorFactor(vector.y, GenLocalDate.DayOfYear(map)));
			}
			Shader.SetGlobalInt("_FallGlobalControls", (!PlantFallColors.FallGlobalControls) ? 0 : 1);
			if (PlantFallColors.FallGlobalControls)
			{
				Shader.SetGlobalVector("_FallSrc", new Vector3(PlantFallColors.FallSrcR, PlantFallColors.FallSrcG, PlantFallColors.FallSrcB));
				Shader.SetGlobalVector("_FallDst", new Vector3(PlantFallColors.FallDstR, PlantFallColors.FallDstG, PlantFallColors.FallDstB));
				Shader.SetGlobalVector("_FallRange", new Vector3(PlantFallColors.FallRangeBegin, PlantFallColors.FallRangeEnd));
			}
		}
	}
}
