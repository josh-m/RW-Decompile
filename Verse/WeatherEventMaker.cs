using System;

namespace Verse
{
	public class WeatherEventMaker
	{
		public float averageInterval = 100f;

		public Type eventClass;

		public void WeatherEventMakerTick(float strength)
		{
			if (Rand.Value < 1f / this.averageInterval * strength)
			{
				WeatherEvent newEvent = (WeatherEvent)Activator.CreateInstance(this.eventClass);
				Find.WeatherManager.eventHandler.AddEvent(newEvent);
			}
		}
	}
}
