using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse.AI;

namespace Verse
{
	public static class BackCompatibility
	{
		public static readonly Pair<int, int>[] SaveCompatibleMinorVersions = new Pair<int, int>[]
		{
			new Pair<int, int>(17, 18)
		};

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
				if (defName == "Neurotrainer")
				{
					return "MechSerumNeurotrainer";
				}
				if (defName == "FueledGenerator")
				{
					return "WoodFiredGenerator";
				}
				if (defName == "Gun_Pistol")
				{
					return "Gun_Revolver";
				}
				if (defName == "Bullet_Pistol")
				{
					return "Bullet_Revolver";
				}
				if (defName == "TableShort")
				{
					return "Table2x2c";
				}
				if (defName == "TableLong")
				{
					return "Table2x4c";
				}
				if (defName == "TableShort_Blueprint")
				{
					return "Table2x2c_Blueprint";
				}
				if (defName == "TableLong_Blueprint")
				{
					return "Table2x4c_Blueprint";
				}
				if (defName == "TableShort_Frame")
				{
					return "Table2x2c_Frame";
				}
				if (defName == "TableLong_Frame")
				{
					return "Table2x4c_Frame";
				}
				if (defName == "TableShort_Install")
				{
					return "Table2x2c_Install";
				}
				if (defName == "TableLong_Install")
				{
					return "Table2x4c_Install";
				}
				if (defName == "Turret_MortarBomb")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_Incendiary")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_MortarIncendiary")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_EMP")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_MortarEMP")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_MortarBomb_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_Incendiary_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_MortarIncendiary_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_EMP_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_MortarEMP_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_MortarBomb_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_Incendiary_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_MortarIncendiary_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_EMP_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_MortarEMP_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_MortarBomb_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_Incendiary_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_MortarIncendiary_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_EMP_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_MortarEMP_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Artillery_MortarBomb")
				{
					return "Artillery_Mortar";
				}
				if (defName == "Artillery_MortarIncendiary")
				{
					return "Artillery_Mortar";
				}
				if (defName == "Artillery_MortarEMP")
				{
					return "Artillery_Mortar";
				}
				if (defName == "TrapIEDBomb")
				{
					return "TrapIED_HighExplosive";
				}
				if (defName == "TrapIEDIncendiary")
				{
					return "TrapIED_Incendiary";
				}
				if (defName == "TrapIEDBomb_Blueprint")
				{
					return "TrapIED_HighExplosive_Blueprint";
				}
				if (defName == "TrapIEDIncendiary_Blueprint")
				{
					return "TrapIED_Incendiary_Blueprint";
				}
				if (defName == "TrapIEDBomb_Frame")
				{
					return "TrapIED_HighExplosive_Frame";
				}
				if (defName == "TrapIEDIncendiary_Frame")
				{
					return "TrapIED_Incendiary_Frame";
				}
				if (defName == "TrapIEDBomb_Install")
				{
					return "TrapIED_HighExplosive_Install";
				}
				if (defName == "TrapIEDIncendiary_Install")
				{
					return "TrapIED_Incendiary_Install";
				}
				if (defName == "Bullet_MortarBomb")
				{
					return "Bullet_Shell_HighExplosive";
				}
				if (defName == "Bullet_MortarIncendiary")
				{
					return "Bullet_Shell_Incendiary";
				}
				if (defName == "Bullet_MortarEMP")
				{
					return "Bullet_Shell_EMP";
				}
				if (defName == "MortarShell")
				{
					return "Shell_HighExplosive";
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
				if (defName == "IEDBomb")
				{
					return "IEDs";
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
					return "Make_Shell_HighExplosive";
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
				if (defName == "Make_Gun_Pistol")
				{
					return "Make_Gun_Revolver";
				}
				if (defName == "Make_TableShort")
				{
					return "Make_Table2x2c";
				}
				if (defName == "Make_TableLong")
				{
					return "Make_Table2x4c";
				}
				if (defName == "MakeMortarShell")
				{
					return "Make_Shell_HighExplosive";
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
				if (defName == "GiftImpact")
				{
					return "DiplomacyPower";
				}
			}
			else if (defType == typeof(SkillDef))
			{
				if (defName == "Research")
				{
					return "Intellectual";
				}
			}
			else if (defType == typeof(LetterDef))
			{
				if (defName == "BadUrgent")
				{
					return "ThreatBig";
				}
				if (defName == "BadNonUrgent")
				{
					return "NegativeEvent";
				}
				if (defName == "Good")
				{
					return "PositiveEvent";
				}
			}
			else if (defType == typeof(WorldObjectDef) && defName == "JourneyDestination")
			{
				return "EscapeShip";
			}
			return defName;
		}

		public static Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			if (baseType == typeof(WorldObject))
			{
				if (providedClassName == "RimWorld.Planet.WorldObject" && node["def"] != null && node["def"].InnerText == "JourneyDestination")
				{
					return WorldObjectDefOf.EscapeShip.worldObjectClass;
				}
			}
			else if (baseType == typeof(Thing))
			{
				if (providedClassName == "Building_PoisonShipPart" && node["def"] != null && node["def"].InnerText == "CrashedPoisonShipPart")
				{
					return ThingDefOf.CrashedPoisonShipPart.thingClass;
				}
				if (providedClassName == "Building_PsychicEmanator" && node["def"] != null && node["def"].InnerText == "CrashedPsychicEmanatorShipPart")
				{
					return ThingDefOf.CrashedPsychicEmanatorShipPart.thingClass;
				}
			}
			return GenTypes.GetTypeInAnyAssembly(providedClassName);
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
			if (map.pawnDestinationReservationManager == null)
			{
				map.pawnDestinationReservationManager = new PawnDestinationReservationManager();
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
					jobTracker.ClearQueuedJobs();
				}
			}
		}

		public static void RecordsTrackerPostLoadInit(Pawn_RecordsTracker recordTracker)
		{
			if (VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) == 0 && VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) <= 17 && Find.TaleManager.AnyTaleConcerns(recordTracker.pawn))
			{
				recordTracker.AccumulateStoryEvent(StoryEventDefOf.TaleCreated);
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

		public static void TurretPostLoadInit(Building_TurretGun turret)
		{
			if (turret.gun == null)
			{
				turret.MakeGun();
			}
		}

		public static void ImportantPawnCompPostLoadInit(ImportantPawnComp c)
		{
			if (c.pawn == null)
			{
				c.pawn = new ThingOwner<Pawn>(c, true, LookMode.Deep);
			}
		}

		public static void PawnPostLoadInit(Pawn p)
		{
			if (p.Spawned && p.rotationTracker == null)
			{
				p.rotationTracker = new Pawn_RotationTracker(p);
			}
		}

		public static void WorldPawnPostLoadInit(WorldPawns wp)
		{
			if (VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) == 0 && VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) <= 17)
			{
				wp.UnpinAllForcefullyKeptPawns();
			}
			if (wp.gc == null)
			{
				wp.gc = new WorldPawnGC();
			}
		}

		public static void MindStatePostLoadInit(Pawn_MindState mindState)
		{
			if (mindState.inspirationHandler == null)
			{
				mindState.inspirationHandler = new InspirationHandler(mindState.pawn);
			}
		}

		public static void GameConditionPostLoadInit(GameCondition gameCondition)
		{
			if (!gameCondition.Permanent && gameCondition.Duration > 1000000000)
			{
				gameCondition.Permanent = true;
			}
		}

		public static void GameLoadingVars(Game game)
		{
			if (game.battleLog == null)
			{
				game.battleLog = new BattleLog();
			}
		}
	}
}
