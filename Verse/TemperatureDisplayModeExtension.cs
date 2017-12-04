using System;

namespace Verse
{
	public static class TemperatureDisplayModeExtension
	{
		public static string ToStringHuman(this TemperatureDisplayMode mode)
		{
			if (mode == TemperatureDisplayMode.Celsius)
			{
				return "Celsius".Translate();
			}
			if (mode == TemperatureDisplayMode.Fahrenheit)
			{
				return "Fahrenheit".Translate();
			}
			if (mode != TemperatureDisplayMode.Kelvin)
			{
				throw new NotImplementedException();
			}
			return "Kelvin".Translate();
		}
	}
}
