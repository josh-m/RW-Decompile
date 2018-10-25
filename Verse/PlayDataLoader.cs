using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace Verse
{
	public static class PlayDataLoader
	{
		private static bool loadedInt;

		public static bool Loaded
		{
			get
			{
				return PlayDataLoader.loadedInt;
			}
		}

		public static void LoadAllPlayData(bool recovering = false)
		{
			if (PlayDataLoader.loadedInt)
			{
				Log.Error("Loading play data when already loaded. Call ClearAllPlayData first.", false);
				return;
			}
			DeepProfiler.Start("LoadAllPlayData");
			try
			{
				PlayDataLoader.DoPlayLoad();
			}
			catch (Exception arg)
			{
				if (!Prefs.ResetModsConfigOnCrash)
				{
					throw;
				}
				if (recovering)
				{
					Log.Warning("Could not recover from errors loading play data. Giving up.", false);
					throw;
				}
				IEnumerable<ModMetaData> activeModsInLoadOrder = ModsConfig.ActiveModsInLoadOrder;
				if (activeModsInLoadOrder.Count<ModMetaData>() == 1 && activeModsInLoadOrder.First<ModMetaData>().IsCoreMod)
				{
					throw;
				}
				Log.Warning("Caught exception while loading play data but there are active mods other than Core. Resetting mods config and trying again.\nThe exception was: " + arg, false);
				try
				{
					PlayDataLoader.ClearAllPlayData();
				}
				catch
				{
					Log.Warning("Caught exception while recovering from errors and trying to clear all play data. Ignoring it.\nThe exception was: " + arg, false);
				}
				ModsConfig.Reset();
				DirectXmlCrossRefLoader.Clear();
				PlayDataLoader.LoadAllPlayData(true);
				return;
			}
			finally
			{
				DeepProfiler.End();
			}
			PlayDataLoader.loadedInt = true;
			if (recovering)
			{
				Log.Message("Successfully recovered from errors and loaded play data.", false);
				DelayedErrorWindowRequest.Add("RecoveredFromErrorsText".Translate(), "RecoveredFromErrorsDialogTitle".Translate());
			}
		}

		private static void DoPlayLoad()
		{
			GraphicDatabase.Clear();
			DeepProfiler.Start("Load all active mods.");
			try
			{
				LoadedModManager.LoadAllActiveMods();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Load language metadata.");
			try
			{
				LanguageDatabase.LoadAllMetadata();
			}
			finally
			{
				DeepProfiler.End();
			}
			LongEventHandler.SetCurrentEventText("LoadingDefs".Translate());
			DeepProfiler.Start("Copy all Defs from mods to global databases.");
			try
			{
				foreach (Type current in typeof(Def).AllSubclasses())
				{
					GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), current, "AddAllInMods");
				}
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve cross-references between non-implied Defs.");
			try
			{
				DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Rebind defs (early).");
			try
			{
				DefOfHelper.RebindAllDefOfs(true);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Inject selected language data into game data (early pass).");
			try
			{
				LanguageDatabase.activeLanguage.InjectIntoData_BeforeImpliedDefs();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Generate implied Defs (pre-resolve).");
			try
			{
				DefGenerator.GenerateImpliedDefs_PreResolve();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve cross-references between Defs made by the implied defs.");
			try
			{
				DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Rebind DefOfs (final).");
			try
			{
				DefOfHelper.RebindAllDefOfs(false);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Other def binding, resetting and global operations (pre-resolve).");
			try
			{
				PlayerKnowledgeDatabase.ReloadAndRebind();
				LessonAutoActivator.Reset();
				CostListCalculator.Reset();
				Pawn.ResetStaticData();
				PawnApparelGenerator.Reset();
				RestUtility.Reset();
				ThoughtUtility.Reset();
				PawnWeaponGenerator.Reset();
				ThinkTreeKeyAssigner.Reset();
				ThingCategoryNodeDatabase.FinalizeInit();
				TrainableUtility.Reset();
				HaulAIUtility.Reset();
				GenConstruct.Reset();
				MedicalCareUtility.Reset();
				InspectPaneUtility.Reset();
				GraphicDatabaseHeadRecords.Reset();
				DateReadout.Reset();
				ResearchProjectDef.GenerateNonOverlappingCoordinates();
				BaseGen.Reset();
				ResourceCounter.ResetDefs();
				ApparelProperties.ResetStaticData();
				WildPlantSpawner.ResetStaticData();
				PawnGenerator.Reset();
				TunnelHiveSpawner.ResetStaticData();
				Hive.ResetStaticData();
				ExpectationsUtility.Reset();
				WealthWatcher.ResetStaticData();
				SkillUI.Reset();
				WorkGiver_FillFermentingBarrel.ResetStaticData();
				WorkGiver_DoBill.ResetStaticData();
				WorkGiver_InteractAnimal.ResetStaticData();
				WorkGiver_Warden_DoExecution.ResetStaticData();
				WorkGiver_GrowerSow.ResetStaticData();
				WorkGiver_Miner.ResetStaticData();
				WorkGiver_FixBrokenDownBuilding.ResetStaticData();
				WorkGiver_ConstructDeliverResources.ResetStaticData();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve references.");
			try
			{
				foreach (Type current2 in typeof(Def).AllSubclasses())
				{
					if (current2 != typeof(ThingDef))
					{
						GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), current2, "ResolveAllReferences", new object[]
						{
							true
						});
					}
				}
				DefDatabase<ThingDef>.ResolveAllReferences(true);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Generate implied Defs (post-resolve).");
			try
			{
				DefGenerator.GenerateImpliedDefs_PostResolve();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Other def binding, resetting and global operations (post-resolve).");
			try
			{
				BuildingProperties.FinalizeInit();
				ThingSetMakerUtility.Reset();
			}
			finally
			{
				DeepProfiler.End();
			}
			if (Prefs.DevMode)
			{
				DeepProfiler.Start("Error check all defs.");
				try
				{
					foreach (Type current3 in typeof(Def).AllSubclasses())
					{
						GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), current3, "ErrorCheckAllDefs");
					}
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			LongEventHandler.SetCurrentEventText("Initializing".Translate());
			DeepProfiler.Start("Load keyboard preferences.");
			try
			{
				KeyPrefs.Init();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Short hash giving.");
			try
			{
				ShortHashGiver.GiveAllShortHashes();
			}
			finally
			{
				DeepProfiler.End();
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("Load backstories.");
				try
				{
					BackstoryDatabase.ReloadAllBackstories();
				}
				finally
				{
					DeepProfiler.End();
				}
			});
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("Inject selected language data into game data.");
				try
				{
					LanguageDatabase.activeLanguage.InjectIntoData_AfterImpliedDefs();
					GenLabel.ClearCache();
				}
				finally
				{
					DeepProfiler.End();
				}
			});
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				StaticConstructorOnStartupUtility.CallAll();
				if (Prefs.DevMode)
				{
					StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes();
				}
			});
		}

		public static void ClearAllPlayData()
		{
			LanguageDatabase.Clear();
			LoadedModManager.ClearDestroy();
			foreach (Type current in typeof(Def).AllSubclasses())
			{
				GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), current, "Clear");
			}
			ThingCategoryNodeDatabase.Clear();
			BackstoryDatabase.Clear();
			SolidBioDatabase.Clear();
			Current.Game = null;
			PlayDataLoader.loadedInt = false;
		}
	}
}
