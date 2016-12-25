using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public sealed class WeatherManager : IExposable
	{
		public const float TransitionTicks = 4000f;

		public Map map;

		public WeatherEventHandler eventHandler = new WeatherEventHandler();

		public WeatherDef curWeather = WeatherDefOf.Clear;

		public WeatherDef lastWeather = WeatherDefOf.Clear;

		public int curWeatherAge;

		private List<Sustainer> ambienceSustainers = new List<Sustainer>();

		public TemperatureMemory growthSeasonMemory;

		public float TransitionLerpFactor
		{
			get
			{
				float num = (float)this.curWeatherAge / 4000f;
				if (num > 1f)
				{
					num = 1f;
				}
				return num;
			}
		}

		public float RainRate
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.rainRate, this.curWeather.rainRate, this.TransitionLerpFactor);
			}
		}

		public float SnowRate
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.snowRate, this.curWeather.snowRate, this.TransitionLerpFactor);
			}
		}

		public float CurWindSpeedFactor
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.windSpeedFactor, this.curWeather.windSpeedFactor, this.TransitionLerpFactor);
			}
		}

		public float CurMoveSpeedMultiplier
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.moveSpeedMultiplier, this.curWeather.moveSpeedMultiplier, this.TransitionLerpFactor);
			}
		}

		public float CurWeatherAccuracyMultiplier
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.accuracyMultiplier, this.curWeather.accuracyMultiplier, this.TransitionLerpFactor);
			}
		}

		public WeatherManager(Map map)
		{
			this.map = map;
			this.growthSeasonMemory = new TemperatureMemory(map);
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<WeatherDef>(ref this.curWeather, "curWeather");
			Scribe_Defs.LookDef<WeatherDef>(ref this.lastWeather, "lastWeather");
			Scribe_Values.LookValue<int>(ref this.curWeatherAge, "curWeatherAge", 0, true);
			Scribe_Deep.LookDeep<TemperatureMemory>(ref this.growthSeasonMemory, "growthSeasonMemory", new object[]
			{
				this.map
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.ambienceSustainers.Clear();
			}
		}

		public void TransitionTo(WeatherDef newWeather)
		{
			this.lastWeather = this.curWeather;
			this.curWeather = newWeather;
			this.curWeatherAge = 0;
		}

		public void DoWeatherGUI(Rect rect)
		{
			Text.Anchor = TextAnchor.MiddleRight;
			Rect rect2 = new Rect(rect);
			rect2.width -= 15f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, this.curWeather.LabelCap);
			if (!this.curWeather.description.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, this.curWeather.description);
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public void WeatherManagerTick()
		{
			this.eventHandler.WeatherEventHandlerTick();
			this.curWeatherAge++;
			this.curWeather.Worker.WeatherTick(this.map, this.TransitionLerpFactor);
			this.lastWeather.Worker.WeatherTick(this.map, 1f - this.TransitionLerpFactor);
			this.growthSeasonMemory.GrowthSeasonMemoryTick();
			for (int i = 0; i < this.curWeather.ambientSounds.Count; i++)
			{
				bool flag = false;
				for (int j = this.ambienceSustainers.Count - 1; j >= 0; j--)
				{
					if (this.ambienceSustainers[j].def == this.curWeather.ambientSounds[i])
					{
						flag = true;
						break;
					}
				}
				if (!flag && this.VolumeOfAmbientSound(this.curWeather.ambientSounds[i]) > 0.0001f)
				{
					SoundInfo info = SoundInfo.OnCamera(MaintenanceType.None);
					Sustainer sustainer = this.curWeather.ambientSounds[i].TrySpawnSustainer(info);
					if (sustainer != null)
					{
						this.ambienceSustainers.Add(sustainer);
					}
				}
			}
		}

		public void WeatherManagerUpdate()
		{
			this.SetAmbienceSustainersVolume();
		}

		private void SetAmbienceSustainersVolume()
		{
			for (int i = this.ambienceSustainers.Count - 1; i >= 0; i--)
			{
				float num = this.VolumeOfAmbientSound(this.ambienceSustainers[i].def);
				if (num > 0.0001f)
				{
					this.ambienceSustainers[i].externalParams["LerpFactor"] = num;
				}
				else
				{
					this.ambienceSustainers[i].End();
					this.ambienceSustainers.RemoveAt(i);
				}
			}
		}

		private float VolumeOfAmbientSound(SoundDef soundDef)
		{
			if (this.map != Find.VisibleMap)
			{
				return 0f;
			}
			for (int i = 0; i < Find.WindowStack.Count; i++)
			{
				if (Find.WindowStack[i].silenceAmbientSound)
				{
					return 0f;
				}
			}
			float num = 0f;
			for (int j = 0; j < this.lastWeather.ambientSounds.Count; j++)
			{
				if (this.lastWeather.ambientSounds[j] == soundDef)
				{
					num += 1f - this.TransitionLerpFactor;
				}
			}
			for (int k = 0; k < this.curWeather.ambientSounds.Count; k++)
			{
				if (this.curWeather.ambientSounds[k] == soundDef)
				{
					num += this.TransitionLerpFactor;
				}
			}
			return num;
		}

		public void DrawAllWeather()
		{
			this.eventHandler.WeatherEventsDraw();
			this.lastWeather.Worker.DrawWeather(this.map);
			this.curWeather.Worker.DrawWeather(this.map);
		}
	}
}
