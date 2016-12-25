using System;
using Verse;

namespace RimWorld
{
	public static class ZonePresetNames
	{
		public static string PresetName(this StorageSettingsPreset preset)
		{
			if (preset == StorageSettingsPreset.DumpingStockpile)
			{
				return "DumpingStockpile".Translate();
			}
			if (preset == StorageSettingsPreset.DefaultStockpile)
			{
				return "Stockpile".Translate();
			}
			return "Zone".Translate();
		}
	}
}
