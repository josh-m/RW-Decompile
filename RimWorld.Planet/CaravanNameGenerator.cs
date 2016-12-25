using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanNameGenerator
	{
		public static string GenerateCaravanName(Caravan caravan)
		{
			for (int i = 1; i <= 1000; i++)
			{
				string text = caravan.def.label + " " + i;
				if (!CaravanNameGenerator.CaravanNameInUse(text))
				{
					return text;
				}
			}
			Log.Error("Ran out of caravan names.");
			return caravan.def.label;
		}

		private static bool CaravanNameInUse(string name)
		{
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (caravans[i].Name == name)
				{
					return true;
				}
			}
			return false;
		}
	}
}
