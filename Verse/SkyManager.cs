using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class SkyManager
	{
		public const float NightMaxCelGlow = 0.1f;

		public const float DuskMaxCelGlow = 0.6f;

		private Map map;

		private float curSkyGlowInt;

		private List<Pair<SkyOverlay, float>> tempOverlays = new List<Pair<SkyOverlay, float>>();

		private static readonly Color FogOfWarBaseColor = new Color32(77, 69, 66, 255);

		public float CurSkyGlow
		{
			get
			{
				return this.curSkyGlowInt;
			}
		}

		public SkyManager(Map map)
		{
			this.map = map;
		}

		public void SkyManagerUpdate()
		{
			SkyTarget curSky = this.CurrentSkyTarget();
			this.curSkyGlowInt = curSky.glow;
			if (this.map == Find.VisibleMap)
			{
				MatBases.LightOverlay.color = curSky.colors.sky;
				Find.CameraColor.saturation = curSky.colors.saturation;
				Color color = curSky.colors.sky;
				color.a = 1f;
				color *= SkyManager.FogOfWarBaseColor;
				MatBases.FogOfWar.color = color;
				Color color2 = curSky.colors.shadow;
				WeatherEvent overridingWeatherEvent = this.map.weatherManager.eventHandler.OverridingWeatherEvent;
				if (overridingWeatherEvent != null && overridingWeatherEvent.OverrideShadowVector.HasValue)
				{
					this.SetSunShadowVector(overridingWeatherEvent.OverrideShadowVector.Value);
				}
				else
				{
					this.SetSunShadowVector(GenCelestial.CurShadowVector(this.map));
					color2 = Color.Lerp(Color.white, color2, GenCelestial.CurShadowStrength(this.map));
				}
				MatBases.SunShadow.color = color2;
				this.UpdateOverlays(curSky);
			}
		}

		private void UpdateOverlays(SkyTarget curSky)
		{
			this.tempOverlays.Clear();
			List<SkyOverlay> overlays = this.map.weatherManager.curWeather.Worker.overlays;
			for (int i = 0; i < overlays.Count; i++)
			{
				this.AddTempOverlay(new Pair<SkyOverlay, float>(overlays[i], this.map.weatherManager.TransitionLerpFactor));
			}
			List<SkyOverlay> overlays2 = this.map.weatherManager.lastWeather.Worker.overlays;
			for (int j = 0; j < overlays2.Count; j++)
			{
				this.AddTempOverlay(new Pair<SkyOverlay, float>(overlays2[j], 1f - this.map.weatherManager.TransitionLerpFactor));
			}
			for (int k = 0; k < this.map.mapConditionManager.ActiveConditions.Count; k++)
			{
				MapCondition mapCondition = this.map.mapConditionManager.ActiveConditions[k];
				List<SkyOverlay> list = mapCondition.SkyOverlays();
				if (list != null)
				{
					for (int l = 0; l < list.Count; l++)
					{
						this.AddTempOverlay(new Pair<SkyOverlay, float>(list[l], mapCondition.SkyTargetLerpFactor()));
					}
				}
			}
			for (int m = 0; m < this.tempOverlays.Count; m++)
			{
				Color overlay = curSky.colors.overlay;
				overlay.a = this.tempOverlays[m].Second;
				this.tempOverlays[m].First.OverlayColor = overlay;
			}
		}

		private void AddTempOverlay(Pair<SkyOverlay, float> pair)
		{
			for (int i = 0; i < this.tempOverlays.Count; i++)
			{
				if (this.tempOverlays[i].First == pair.First)
				{
					this.tempOverlays[i] = new Pair<SkyOverlay, float>(this.tempOverlays[i].First, Mathf.Clamp01(this.tempOverlays[i].Second + pair.Second));
					return;
				}
			}
			this.tempOverlays.Add(pair);
		}

		private void SetSunShadowVector(Vector2 vec)
		{
			MatBases.SunShadow.SetVector("_CastVect", new Vector4(vec.x, 0f, vec.y, 0f));
		}

		private SkyTarget CurrentSkyTarget()
		{
			SkyTarget b = this.map.weatherManager.curWeather.Worker.CurSkyTarget(this.map);
			SkyTarget a = this.map.weatherManager.lastWeather.Worker.CurSkyTarget(this.map);
			SkyTarget skyTarget = SkyTarget.Lerp(a, b, this.map.weatherManager.TransitionLerpFactor);
			float num = this.map.mapConditionManager.AggregateSkyTargetLerpFactor();
			if (num > 0.0001f)
			{
				SkyTarget value = this.map.mapConditionManager.AggregateSkyTarget().Value;
				skyTarget = SkyTarget.Lerp(skyTarget, value, num);
			}
			WeatherEvent overridingWeatherEvent = this.map.weatherManager.eventHandler.OverridingWeatherEvent;
			if (overridingWeatherEvent != null)
			{
				skyTarget = SkyTarget.Lerp(skyTarget, overridingWeatherEvent.SkyTarget, overridingWeatherEvent.SkyTargetLerpFactor);
			}
			return skyTarget;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("SkyManager: ");
			stringBuilder.AppendLine("CurCelestialSunGlow: " + GenCelestial.CurCelestialSunGlow(Find.VisibleMap));
			stringBuilder.AppendLine("CurSkyGlow: " + this.CurSkyGlow.ToStringPercent());
			stringBuilder.AppendLine("CurrentSkyTarget: " + this.CurrentSkyTarget().ToString());
			return stringBuilder.ToString();
		}
	}
}
