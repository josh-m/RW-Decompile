using System;
using Verse;

namespace RimWorld
{
	public class Dialog_MapList_Load : Dialog_MapList
	{
		public Dialog_MapList_Load()
		{
			this.interactButLabel = "LoadGameButton".Translate();
		}

		protected override void DoFileInteraction(string mapName)
		{
			PreLoadUtility.CheckVersionAndLoad(GenFilePaths.FilePathForSavedGame(mapName), ScribeMetaHeaderUtility.ScribeHeaderMode.Map, delegate
			{
				Action preLoadLevelAction = delegate
				{
					Current.Game = new Game();
					Current.Game.InitData = new GameInitData();
					Current.Game.InitData.mapToLoad = mapName;
				};
				LongEventHandler.QueueLongEvent(preLoadLevelAction, "Map", "LoadingLongEvent", true, null);
			});
		}
	}
}
