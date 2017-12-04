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

		private const int FirstWeatherDuration = 10000;

		private const float ChanceFactorRainOnFire = 15f;

		public WeatherDecider(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.curWeatherDuration, "curWeatherDuration", 0, true);
			Scribe_Values.Look<int>(ref this.ticksWhenRainAllowedAgain, "ticksWhenRainAllowedAgain", 0, false);
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

		public void StartInitialWeather()
		{
			if (Find.GameInitData != null)
			{
				this.map.weatherManager.curWeather = WeatherDefOf.Clear;
				this.curWeatherDuration = 10000;
				this.map.weatherManager.curWeatherAge = 0;
			}
			else
			{
				this.map.weatherManager.curWeather = null;
				WeatherDef weatherDef = this.ChooseNextWeather();
				WeatherDef lastWeather = this.ChooseNextWeather();
				this.map.weatherManager.curWeather = weatherDef;
				this.map.weatherManager.lastWeather = lastWeather;
				this.curWeatherDuration = weatherDef.durationRange.RandomInRange;
				this.map.weatherManager.curWeatherAge = Rand.Range(0, this.curWeatherDuration);
			}
		}

		private WeatherDef ChooseNextWeather()
		{
			if (TutorSystem.TutorialMode)
			{
				return WeatherDefOf.Clear;
			}
			WeatherDef result;
			if (!DefDatabase<WeatherDef>.AllDefs.TryRandomElementByWeight((WeatherDef w) => this.CurrentWeatherCommonality(w), out result))
			{
				Log.Warning("All weather commonalities were zero. Defaulting to " + WeatherDefOf.Clear.defName + ".");
				return WeatherDefOf.Clear;
			}
			return result;
		}

		public void DisableRainFor(int ticks)
		{
			this.ticksWhenRainAllowedAgain = Find.TickManager.TicksGame + ticks;
		}

		private float CurrentWeatherCommonality(WeatherDef weather)
		{
			if (this.map.weatherManager.curWeather != null && !this.map.weatherManager.curWeather.repeatable && weather == this.map.weatherManager.curWeather)
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
				if (this.map.gameConditionManager.ActiveConditions.Any((GameCondition x) => x.def.preventRain))
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
