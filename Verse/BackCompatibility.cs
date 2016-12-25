using RimWorld;
using System;

namespace Verse
{
	public static class BackCompatibility
	{
		public static string BackCompatibleDefName(Type defType, string defName)
		{
			if (defType == typeof(ThingDef))
			{
				if (defName == "Crack")
				{
					return "Flake";
				}
				if (defName == "Gun_PDW")
				{
					return "Gun_MachinePistol";
				}
				if (defName == "Bullet_PDW")
				{
					return "Bullet_MachinePistol";
				}
				if (defName == "Components")
				{
					return "Component";
				}
			}
			else if (defType == typeof(HediffDef))
			{
				if (defName == "Alcohol")
				{
					return "AlcoholHigh";
				}
			}
			else if (defType == typeof(ResearchProjectDef))
			{
				if (defName == "CrackProduction")
				{
					return "FlakeProduction";
				}
			}
			else if (defType == typeof(MentalStateDef))
			{
				if (defName == "ConfusedWander")
				{
					return "WanderConfused";
				}
				if (defName == "DazedWander")
				{
					return "WanderPsychotic";
				}
			}
			else if (defType == typeof(RulePackDef))
			{
				if (defName == "NamerAnimalGeneric")
				{
					return "NamerAnimalGenericMale";
				}
			}
			else if (defType == typeof(TraderKindDef))
			{
				if (defName == "Caravan_Neolithic_SlavesMerchant")
				{
					return "Caravan_Neolithic_Slaver";
				}
			}
			else if (defType == typeof(DifficultyDef))
			{
				if (defName == "FreePlay")
				{
					return "VeryEasy";
				}
				if (defName == "Basebuilder")
				{
					return "Easy";
				}
				if (defName == "Rough")
				{
					return "Medium";
				}
				if (defName == "Challenge")
				{
					return "Hard";
				}
				if (defName == "Extreme")
				{
					return "VeryHard";
				}
			}
			return defName;
		}

		public static string BackCompatibleModifiedTranslationPath(Type defType, string path)
		{
			if (defType == typeof(ConceptDef) && path.Contains("helpTexts.0"))
			{
				return path.Replace("helpTexts.0", "helpText");
			}
			return path;
		}
	}
}
