using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class SkyManager
	{
		public const float NightMaxCelGlow = 0.1f;

		public const float DuskMaxCelGlow = 0.6f;

		private static float curSkyGlowInt;

		private static List<Pair<SkyOverlay, float>> tempOverlays = new List<Pair<SkyOverlay, float>>();

		private static readonly Color FogOfWarBaseColor = new Color32(77, 69, 66, 255);

		public static float CurSkyGlow
		{
			get
			{
				return SkyManager.curSkyGlowInt;
			}
		}

		public static void SkyManagerUpdate()
		{
			SkyTarget skyTarget = SkyManager.CurrentSkyTarget();
			SkyManager.curSkyGlowInt = skyTarget.glow;
			MatBases.LightOverlay.color = skyTarget.colors.sky;
			Find.CameraColor.saturation = skyTarget.colors.saturation;
			Color color = skyTarget.colors.sky;
			color.a = 1f;
			color *= SkyManager.FogOfWarBaseColor;
			MatBases.FogOfWar.color = color;
			Color color2 = skyTarget.colors.shadow;
			WeatherEvent overridingWeatherEvent = Find.WeatherManager.eventHandler.OverridingWeatherEvent;
			if (overridingWeatherEvent != null && overridingWeatherEvent.OverrideShadowVector.HasValue)
			{
				SkyManager.SetSunShadowVector(overridingWeatherEvent.OverrideShadowVector.Value);
			}
			else
			{
				SkyManager.SetSunShadowVector(GenCelestial.CurShadowVector());
				color2 = Color.Lerp(Color.white, color2, GenCelestial.CurShadowStrength());
			}
			MatBases.SunShadow.color = color2;
			SkyManager.tempOverlays.Clear();
			List<SkyOverlay> overlays = Find.WeatherManager.curWeather.Worker.overlays;
			for (int i = 0; i < overlays.Count; i++)
			{
				SkyManager.AddTempOverlay(new Pair<SkyOverlay, float>(overlays[i], Find.WeatherManager.TransitionLerpFactor));
			}
			List<SkyOverlay> overlays2 = Find.WeatherManager.lastWeather.Worker.overlays;
			for (int j = 0; j < overlays2.Count; j++)
			{
				SkyManager.AddTempOverlay(new Pair<SkyOverlay, float>(overlays2[j], 1f - Find.WeatherManager.TransitionLerpFactor));
			}
			for (int k = 0; k < Find.MapConditionManager.ActiveConditions.Count; k++)
			{
				MapCondition mapCondition = Find.MapConditionManager.ActiveConditions[k];
				List<SkyOverlay> list = mapCondition.SkyOverlays();
				if (list != null)
				{
					for (int l = 0; l < list.Count; l++)
					{
						SkyManager.AddTempOverlay(new Pair<SkyOverlay, float>(list[l], mapCondition.SkyTargetLerpFactor()));
					}
				}
			}
			for (int m = 0; m < SkyManager.tempOverlays.Count; m++)
			{
				Color overlay = skyTarget.colors.overlay;
				overlay.a = SkyManager.tempOverlays[m].Second;
				SkyManager.tempOverlays[m].First.OverlayColor = overlay;
			}
		}

		private static void AddTempOverlay(Pair<SkyOverlay, float> pair)
		{
			for (int i = 0; i < SkyManager.tempOverlays.Count; i++)
			{
				if (SkyManager.tempOverlays[i].First == pair.First)
				{
					SkyManager.tempOverlays[i] = new Pair<SkyOverlay, float>(SkyManager.tempOverlays[i].First, Mathf.Clamp01(SkyManager.tempOverlays[i].Second + pair.Second));
					return;
				}
			}
			SkyManager.tempOverlays.Add(pair);
		}

		public static void SetSunShadowVector(Vector2 vec)
		{
			MatBases.SunShadow.SetVector("_CastVect", new Vector4(vec.x, 0f, vec.y, 0f));
		}

		private static SkyTarget CurrentSkyTarget()
		{
			SkyTarget b = Find.WeatherManager.curWeather.Worker.CurSkyTarget();
			SkyTarget a = Find.WeatherManager.lastWeather.Worker.CurSkyTarget();
			SkyTarget skyTarget = SkyTarget.Lerp(a, b, Find.WeatherManager.TransitionLerpFactor);
			float num = Find.MapConditionManager.AggregateSkyTargetLerpFactor();
			if (num > 0.0001f)
			{
				SkyTarget value = Find.MapConditionManager.AggregateSkyTarget().Value;
				skyTarget = SkyTarget.Lerp(skyTarget, value, num);
			}
			WeatherEvent overridingWeatherEvent = Find.WeatherManager.eventHandler.OverridingWeatherEvent;
			if (overridingWeatherEvent != null)
			{
				skyTarget = SkyTarget.Lerp(skyTarget, overridingWeatherEvent.SkyTarget, overridingWeatherEvent.SkyTargetLerpFactor);
			}
			return skyTarget;
		}

		public static string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("SkyManager: ");
			stringBuilder.AppendLine("CurCelestialSunGlow: " + GenCelestial.CurCelestialSunGlow());
			stringBuilder.AppendLine("CurSkyGlow: " + SkyManager.CurSkyGlow.ToStringPercent());
			stringBuilder.AppendLine("CurrentSkyTarget: " + SkyManager.CurrentSkyTarget().ToString());
			return stringBuilder.ToString();
		}
	}
}
