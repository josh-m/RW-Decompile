using RimWorld;
using System;
using System.IO;

namespace Verse
{
	public static class GameDataSaveLoader
	{
		public const string SavedScenarioParentNodeName = "savedscenario";

		public const string SavedWorldParentNodeName = "savedworld";

		public const string SavedGameParentNodeName = "savegame";

		public const string GameNodeName = "game";

		public const string WorldNodeName = "world";

		public const string ScenarioNodeName = "scenario";

		private static int lastSaveTick = -9999;

		public static bool CurrentGameStateIsValuable
		{
			get
			{
				return Find.TickManager.TicksGame > GameDataSaveLoader.lastSaveTick + 60;
			}
		}

		public static void SaveScenario(Scenario scen, string absFilePath)
		{
			try
			{
				scen.fileName = Path.GetFileNameWithoutExtension(absFilePath);
				SafeSaver.Save(absFilePath, "savedscenario", delegate
				{
					ScribeMetaHeaderUtility.WriteMetaHeader();
					Scribe_Deep.LookDeep<Scenario>(ref scen, "scenario", new object[0]);
				});
			}
			catch (Exception ex)
			{
				Log.Error("Exception while saving world: " + ex.ToString());
			}
		}

		public static bool TryLoadScenario(string absPath, ScenarioCategory category, out Scenario scen)
		{
			scen = null;
			try
			{
				Scribe.InitLoading(absPath);
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Scenario, true);
				Scribe_Deep.LookDeep<Scenario>(ref scen, "scenario", new object[0]);
				CrossRefResolver.ResolveAllCrossReferences();
				PostLoadInitter.DoAllPostLoadInits();
				scen.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
				scen.Category = category;
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading scenario: " + ex.ToString());
				scen = null;
				Scribe.ForceStop();
			}
			return scen != null;
		}

		public static void SaveGame(string fileName)
		{
			try
			{
				string path = GenFilePaths.FilePathForSavedGame(fileName);
				SafeSaver.Save(path, "savegame", delegate
				{
					ScribeMetaHeaderUtility.WriteMetaHeader();
					Game game = Current.Game;
					Scribe_Deep.LookDeep<Game>(ref game, "game", new object[0]);
				});
				GameDataSaveLoader.lastSaveTick = Find.TickManager.TicksGame;
			}
			catch (Exception ex)
			{
				Log.Error("Exception while saving map: " + ex.ToString());
			}
		}
	}
}
