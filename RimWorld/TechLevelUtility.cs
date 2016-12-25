using System;
using Verse;

namespace RimWorld
{
	public static class TechLevelUtility
	{
		public static string ToStringHuman(this TechLevel tl)
		{
			switch (tl)
			{
			case TechLevel.Undefined:
				return "Undefined".Translate();
			case TechLevel.Animal:
				return "TechLevel_Animal".Translate();
			case TechLevel.Neolithic:
				return "TechLevel_Neolithic".Translate();
			case TechLevel.Medieval:
				return "TechLevel_Medieval".Translate();
			case TechLevel.Industrial:
				return "TechLevel_Industrial".Translate();
			case TechLevel.Spacer:
				return "TechLevel_Spacer".Translate();
			case TechLevel.Ultra:
				return "TechLevel_Ultra".Translate();
			case TechLevel.Transcendent:
				return "TechLevel_Transcendent".Translate();
			default:
				throw new NotImplementedException();
			}
		}

		public static bool CanSpawnWithEquipmentFrom(this TechLevel pawnLevel, TechLevel gearLevel)
		{
			if (gearLevel == TechLevel.Undefined)
			{
				return false;
			}
			switch (pawnLevel)
			{
			case TechLevel.Undefined:
				return false;
			case TechLevel.Neolithic:
				return gearLevel <= TechLevel.Neolithic;
			case TechLevel.Medieval:
				return gearLevel <= TechLevel.Medieval;
			case TechLevel.Industrial:
				return gearLevel == TechLevel.Industrial;
			case TechLevel.Spacer:
				return gearLevel == TechLevel.Spacer || gearLevel == TechLevel.Industrial;
			case TechLevel.Ultra:
				return gearLevel == TechLevel.Ultra || gearLevel == TechLevel.Spacer;
			case TechLevel.Transcendent:
				return gearLevel == TechLevel.Transcendent;
			}
			Log.Error(string.Concat(new object[]
			{
				"Unknown tech levels ",
				pawnLevel,
				", ",
				gearLevel
			}));
			return true;
		}

		public static bool IsNeolithicOrWorse(this TechLevel techLevel)
		{
			return techLevel != TechLevel.Undefined && techLevel <= TechLevel.Neolithic;
		}
	}
}
