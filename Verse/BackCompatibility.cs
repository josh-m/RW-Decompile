using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class BackCompatibility
	{
		public static readonly Pair<int, int>[] SaveCompatibleMinorVersions = new Pair<int, int>[0];

		public static bool IsSaveCompatibleWith(string version)
		{
			if (VersionControl.BuildFromVersionString(version) == VersionControl.CurrentBuild)
			{
				return true;
			}
			if (VersionControl.MajorFromVersionString(version) != 0 || VersionControl.CurrentMajor != 0)
			{
				return false;
			}
			int num = VersionControl.MinorFromVersionString(version);
			int currentMinor = VersionControl.CurrentMinor;
			for (int i = 0; i < BackCompatibility.SaveCompatibleMinorVersions.Length; i++)
			{
				if (BackCompatibility.SaveCompatibleMinorVersions[i].First == num && BackCompatibility.SaveCompatibleMinorVersions[i].Second == currentMinor)
				{
					return true;
				}
			}
			return false;
		}

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
				if (defName == "EquipmentRack")
				{
					return "Shelf";
				}
				if (defName == "Apparel_MilitaryHelmet")
				{
					return "Apparel_SimpleHelmet";
				}
				if (defName == "Apparel_KevlarHelmet")
				{
					return "Apparel_AdvancedHelmet";
				}
				if (defName == "Apparel_PersonalShield")
				{
					return "Apparel_ShieldBelt";
				}
				if (defName == "MuffaloWool")
				{
					return "WoolMuffalo";
				}
				if (defName == "MegaslothWool")
				{
					return "WoolMegasloth";
				}
				if (defName == "AlpacaWool")
				{
					return "WoolAlpaca";
				}
				if (defName == "CamelHair")
				{
					return "WoolCamel";
				}
				if (defName == "Gun_SurvivalRifle")
				{
					return "Gun_BoltActionRifle";
				}
				if (defName == "Bullet_SurvivalRifle")
				{
					return "Bullet_BoltActionRifle";
				}
			}
			else if (defType == typeof(ConceptDef))
			{
				if (defName == "PersonalShields")
				{
					return "ShieldBelts";
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
					return "PenoxycylineProduction";
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
				if (defName == "Make_Apparel_MilitaryHelmet")
				{
					return "Make_Apparel_SimpleHelmet";
				}
				if (defName == "Make_Apparel_KevlarHelmet")
				{
					return "Make_Apparel_AdvancedHelmet";
				}
				if (defName == "Make_Gun_SurvivalRifle")
				{
					return "Make_Gun_BoltActionRifle";
				}
			}
			else if (defType == typeof(HediffDef))
			{
				if (defName == "Euthanasia")
				{
					return "ShutDown";
				}
				if (defName == "ChemicalDamageBrain")
				{
					return "ChemicalDamageModerate";
				}
				if (defName == "ChemicalDamageKidney")
				{
					return "ChemicalDamageSevere";
				}
			}
			else if (defType == typeof(TraderKindDef))
			{
				if (defName == "Caravan_Neolithic_CombatSupplier")
				{
					return "Caravan_Neolithic_WarMerchant";
				}
			}
			else if (defType == typeof(StatDef))
			{
				if (defName == "HarvestYield")
				{
					return "PlantHarvestYield";
				}
				if (defName == "SurgerySuccessChance")
				{
					return "MedicalSurgerySuccessChance";
				}
				if (defName == "HealingQuality")
				{
					return "MedicalTendQuality";
				}
				if (defName == "HealingSpeed")
				{
					return "MedicalTendSpeed";
				}
			}
			else if (defType == typeof(SkillDef) && defName == "Research")
			{
				return "Intellectual";
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

		public static void AfterLoadingSmallGameClassComponents(Game game)
		{
			if (game.dateNotifier == null)
			{
				game.dateNotifier = new DateNotifier();
			}
			if (game.components == null)
			{
				game.components = new List<GameComponent>();
			}
		}

		public static void FactionBasePostLoadInit(FactionBase factionBase)
		{
			if (factionBase.trader == null)
			{
				factionBase.trader = new FactionBase_TraderTracker(factionBase);
			}
		}

		public static void MapPostLoadInit(Map map)
		{
			if (map.storyState == null)
			{
				map.storyState = new StoryState(map);
			}
		}

		public static void CaravanPostLoadInit(Caravan caravan)
		{
			if (caravan.storyState == null)
			{
				caravan.storyState = new StoryState(caravan);
			}
		}

		public static void SettlementPostLoadInit(Settlement settlement)
		{
			if (settlement.previouslyGeneratedInhabitants == null)
			{
				settlement.previouslyGeneratedInhabitants = new List<Pawn>();
			}
		}

		public static void JobTrackerPostLoadInit(Pawn_JobTracker jobTracker)
		{
			if (jobTracker.jobQueue != null)
			{
				bool flag = false;
				for (int i = 0; i < jobTracker.jobQueue.Count; i++)
				{
					if (jobTracker.jobQueue[i].job == null)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					jobTracker.jobQueue.Clear();
				}
			}
		}

		public static void WorldLoadingVars(World world)
		{
			if (world.components == null)
			{
				world.components = new List<WorldComponent>();
			}
			if (world.settings == null)
			{
				world.settings = new WorldSettings();
			}
		}
	}
}
