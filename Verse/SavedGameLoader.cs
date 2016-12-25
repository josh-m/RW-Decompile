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
			Log.Message("Initializing map from file " + fileName + " with mods " + str);
			DeepProfiler.Start("Loading map from file " + fileName);
			Current.Game = new Game();
			DeepProfiler.Start("InitLoading (read file)");
			Current.ProgramState = ProgramState.MapInitializing;
			RegionAndRoomUpdater.Enabled = false;
			Scribe.InitLoading(GenFilePaths.FilePathForSavedGame(fileName));
			DeepProfiler.End();
			ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
			Scribe.EnterNode("game");
			Current.Game = new Game();
			Current.Game.LoadData();
			if (Prefs.PauseOnLoad)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					Find.TickManager.DoSingleTick();
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				});
			}
			PermadeathModeUtility.CheckUpdatePermadeathModeUniqueNameOnGameLoad(fileName);
			DeepProfiler.End();
		}
	}
}
