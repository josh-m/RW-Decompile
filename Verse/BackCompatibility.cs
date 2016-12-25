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
				if (defName == "Megatherium")
				{
					return "Megasloth";
				}
				if (defName == "MegatheriumWool")
				{
					return "MegaslothWool";
				}
				if (defName == "MalariBlock")
				{
					return "Penoxycyline";
				}
				if (defName == "ArtilleryShell")
				{
					return "MortarShell";
				}
			}
			else if (defType == typeof(PawnKindDef))
			{
				if (defName == "Megatherium")
				{
					return "Megasloth";
				}
			}
			else if (defType == typeof(ThoughtDef))
			{
				if (defName == "ComfortLevel")
				{
					return "NeedComfort";
				}
				if (defName == "JoyLevel")
				{
					return "NeedJoy";
				}
				if (defName == "Tired")
				{
					return "NeedRest";
				}
				if (defName == "Hungry")
				{
					return "NeedFood";
				}
			}
			else if (defType == typeof(ResearchProjectDef))
			{
				if (defName == "MalariBlockProduction")
				{
					return "PenoxycyclineProduction";
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
			else if (defType == typeof(RecipeDef))
			{
				if (defName == "MakeArtilleryShell")
				{
					return "MakeMortarShell";
				}
				if (defName == "Make_MalariBlock")
				{
					return "Make_Penoxycyline";
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
