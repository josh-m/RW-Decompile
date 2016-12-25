using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class WeatherPartPool
	{
		private static List<SkyOverlay> instances = new List<SkyOverlay>();

		public static SkyOverlay GetInstanceOf<T>() where T : SkyOverlay
		{
			for (int i = 0; i < WeatherPartPool.instances.Count; i++)
			{
				T t = WeatherPartPool.instances[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			SkyOverlay skyOverlay = Activator.CreateInstance<T>();
			WeatherPartPool.instances.Add(skyOverlay);
			return skyOverlay;
		}
	}
}
