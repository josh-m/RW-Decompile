using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class WeatherWorker
	{
		private struct SkyThreshold
		{
			public SkyColorSet colors;

			public float celGlowThreshold;

			public SkyThreshold(SkyColorSet colors, float celGlowThreshold)
			{
				this.colors = colors;
				this.celGlowThreshold = celGlowThreshold;
			}
		}

		private WeatherDef def;

		public List<SkyOverlay> overlays = new List<SkyOverlay>();

		private WeatherWorker.SkyThreshold[] skyTargets = new WeatherWorker.SkyThreshold[4];

		public WeatherWorker(WeatherDef def)
		{
			this.def = def;
			foreach (Type current in def.overlayClasses)
			{
				SkyOverlay item = (SkyOverlay)GenGeneric.InvokeStaticGenericMethod(typeof(WeatherPartPool), current, "GetInstanceOf");
				this.overlays.Add(item);
			}
			this.skyTargets[0] = new WeatherWorker.SkyThreshold(def.skyColorsNightMid, 0f);
			this.skyTargets[1] = new WeatherWorker.SkyThreshold(def.skyColorsNightEdge, 0.1f);
			this.skyTargets[2] = new WeatherWorker.SkyThreshold(def.skyColorsDusk, 0.6f);
			this.skyTargets[3] = new WeatherWorker.SkyThreshold(def.skyColorsDay, 1f);
		}

		public void DrawWeather(Map map)
		{
			for (int i = 0; i < this.overlays.Count; i++)
			{
				this.overlays[i].DrawOverlay(map);
			}
		}

		public void WeatherTick(Map map, float lerpFactor)
		{
			for (int i = 0; i < this.overlays.Count; i++)
			{
				this.overlays[i].TickOverlay(map);
			}
			for (int j = 0; j < this.def.eventMakers.Count; j++)
			{
				this.def.eventMakers[j].WeatherEventMakerTick(map, lerpFactor);
			}
		}

		public SkyTarget CurSkyTarget(Map map)
		{
			float num = GenCelestial.CurCelestialSunGlow(map);
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < this.skyTargets.Length; i++)
			{
				num3 = i;
				if (num + 0.001f < this.skyTargets[i].celGlowThreshold)
				{
					break;
				}
				num2 = i;
			}
			WeatherWorker.SkyThreshold skyThreshold = this.skyTargets[num2];
			WeatherWorker.SkyThreshold skyThreshold2 = this.skyTargets[num3];
			float num4 = skyThreshold2.celGlowThreshold - skyThreshold.celGlowThreshold;
			float t;
			if (num4 == 0f)
			{
				t = 1f;
			}
			else
			{
				t = (num - skyThreshold.celGlowThreshold) / num4;
			}
			return new SkyTarget
			{
				glow = num,
				colors = SkyColorSet.Lerp(skyThreshold.colors, skyThreshold2.colors, t)
			};
		}
	}
}
