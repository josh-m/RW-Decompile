using RimWorld;
using RimWorld.Planet;
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

		public const string MapNodeName = "map";

		public const string ScenarioNodeName = "scenario";

		private static int lastSaveTick = -9999;

		public static bool CurrentMapStateIsValuable
		{
			get
			{
				return Find.Map != null && Find.TickManager.TicksGame > GameDataSaveLoader.lastSaveTick + 60;
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

		public static Scenario LoadScenario(string absPath, ScenarioCategory category)
		{
			Scribe.InitLoading(absPath);
			ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Scenario, true);
			Scenario scenario = null;
			Scribe_Deep.LookDeep<Scenario>(ref scenario, "scenario", new object[0]);
			CrossRefResolver.ResolveAllCrossReferences();
			PostLoadInitter.DoAllPostLoadInits();
			scenario.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
			scenario.Category = category;
			return scenario;
		}

		public static void SaveWorld(World world, string fileName)
		{
			try
			{
				string path = GenFilePaths.FilePathForWorld(fileName);
				SafeSaver.Save(path, "savedworld", delegate
				{
					ScribeMetaHeaderUtility.WriteMetaHeader();
					Scribe_Deep.LookDeep<World>(ref world, "world", new object[0]);
				});
			}
			catch (Exception ex)
			{
				Log.Error("Exception while saving world: " + ex.ToString());
			}
		}

		public static void LoadWorldFromFileIntoGame(string filePath)
		{
			Scribe.InitLoading(filePath);
			ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.World, true);
			Current.Game.World = new World();
			Scribe.EnterNode("world");
			Find.World.ExposeData();
			Scribe.ExitNode();
			CrossRefResolver.ResolveAllCrossReferences();
			PostLoadInitter.DoAllPostLoadInits();
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
