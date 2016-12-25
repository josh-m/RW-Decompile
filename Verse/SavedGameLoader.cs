using System;
using System.Linq;

namespace Verse
{
	public class SavedGameLoader
	{
		public static void LoadGameFromSaveFile(string fileName)
		{
			string str = GenText.ToCommaList(from mod in LoadedModManager.RunningMods
			select mod.ToString(), true);
			Log.Message("Loading game from file " + fileName + " with mods " + str);
			DeepProfiler.Start("Loading game from file " + fileName);
			Current.Game = new Game();
			DeepProfiler.Start("InitLoading (read file)");
			Scribe.InitLoading(GenFilePaths.FilePathForSavedGame(fileName));
			DeepProfiler.End();
			ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
			if (Scribe.EnterNode("game"))
			{
				Current.Game = new Game();
				Current.Game.LoadGame();
				PermadeathModeUtility.CheckUpdatePermadeathModeUniqueNameOnGameLoad(fileName);
				DeepProfiler.End();
				return;
			}
			Log.Error("Could not find game XML node.");
		}
	}
}
