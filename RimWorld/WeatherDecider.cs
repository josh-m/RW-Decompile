using System;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class WeatherDecider : IExposable
	{
		private Map map;

		private int curWeatherDuration = 10000;

		private int ticksWhenRainAllowedAgain;

		public WeatherDecider(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.curWeatherDuration, "curWeatherDuration", 0, true);
			Scribe_Values.LookValue<int>(ref this.ticksWhenRainAllowedAgain, "ticksWhenRainAllowedAgain", 0, false);
		}

		public void WeatherDeciderTick()
		{
			int num = this.curWeatherDuration;
			bool flag = this.map.fireWatcher.LargeFireDangerPresent || !this.map.weatherManager.curWeather.temperatureRange.Includes(this.map.mapTemperature.OutdoorTemp);
			if (flag)
			{
				num = (int)((float)num * 0.25f);
			}
			if (this.map.weatherManager.curWeatherAge > num)
			{
				this.StartNextWeather();
			}
		}

		public void StartNextWeather()
		{
			WeatherDef weatherDef = this.ChooseNextWeather();
			this.map.weatherManager.TransitionTo(weatherDef);
			this.curWeatherDuration = weatherDef.durationRange.RandomInRange;
		}

		private WeatherDef ChooseNextWeather()
		{
			if (TutorSystem.TutorialMode)
			{
				return WeatherDefOf.Clear;
			}
			return DefDatabase<WeatherDef>.AllDefs.RandomElementByWeight((WeatherDef w) => this.CurrentWeatherCommonality(w));
		}

		public void DisableRainFor(int ticks)
		{
			this.ticksWhenRainAllowedAgain = Find.TickManager.TicksGame + ticks;
		}

		private float CurrentWeatherCommonality(WeatherDef weather)
		{
			if (!this.map.weatherManager.curWeather.repeatable && weather == this.map.weatherManager.curWeather)
			{
				return 0f;
			}
			if (!weather.temperatureRange.Includes(this.map.mapTemperature.OutdoorTemp))
			{
				return 0f;
			}
			if (weather.favorability < Favorability.Neutral && GenDate.DaysPassed < 8)
			{
				return 0f;
			}
			if (weather.rainRate > 0.1f && Find.TickManager.TicksGame < this.ticksWhenRainAllowedAgain)
			{
				return 0f;
			}
			if (weather.rainRate > 0.1f)
			{
				if (this.map.mapConditionManager.ActiveConditions.Any((MapCondition x) => x.def.preventRain))
				{
					return 0f;
				}
			}
			BiomeDef biome = this.map.Biome;
			for (int i = 0; i < biome.baseWeatherCommonalities.Count; i++)
			{
				WeatherCommonalityRecord weatherCommonalityRecord = biome.baseWeatherCommonalities[i];
				if (weatherCommonalityRecord.weather == weather)
				{
					float num = weatherCommonalityRecord.commonality;
					if (this.map.fireWatcher.LargeFireDangerPresent && weather.rainRate > 0.1f)
					{
						num *= 15f;
					}
					if (weatherCommonalityRecord.weather.commonalityRainfallFactor != null)
					{
						num *= weatherCommonalityRecord.weather.commonalityRainfallFactor.Evaluate(this.map.TileInfo.rainfall);
					}
					return num;
				}
			}
			return 0f;
		}

		public void LogWeatherChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (WeatherDef current in from w in DefDatabase<WeatherDef>.AllDefs
			orderby this.CurrentWeatherCommonality(w) descending
			select w)
			{
				stringBuilder.AppendLine(current.label + " - " + this.CurrentWeatherCommonality(current).ToString());
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
