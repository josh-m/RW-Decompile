using RimWorld;
using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class RootMap : Root
	{
		public override void Start()
		{
			base.Start();
			if (Find.GameInitData != null && !Find.GameInitData.mapToLoad.NullOrEmpty())
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					SavedGameLoader.LoadGameFromSaveFile(Find.GameInitData.mapToLoad);
				}, "LoadingLongEvent", true, new Action<Exception>(RootMap.ErrorWhileLoadingMap));
			}
			else
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					MapIniter_NewGame.InitNewGeneratedMap();
				}, "GeneratingMap", true, new Action<Exception>(RootMap.ErrorWhileGeneratingMap));
			}
			LongEventHandler.QueueLongEvent(delegate
			{
				ScreenFader.SetColor(Color.black);
				ScreenFader.StartFade(Color.clear, 0.5f);
			}, null, false, null);
		}

		public override void Update()
		{
			base.Update();
			try
			{
				if (!LongEventHandler.ShouldWaitForEvent)
				{
					SkyManager.SkyManagerUpdate();
					ShipCountdown.ShipCountdownUpdate();
					Current.Game.Update();
				}
			}
			catch (Exception e)
			{
				Log.Notify_Exception(e);
				throw;
			}
		}

		public static void GoToMainMenu()
		{
			LongEventHandler.ClearQueuedEvents();
			LongEventHandler.QueueLongEvent(delegate
			{
				Current.Game = null;
			}, "Entry", "LoadingLongEvent", true, null);
		}

		private static void ErrorWhileLoadingAssets(Exception e)
		{
			string text = "ErrorWhileLoadingAssets".Translate();
			if (ModsConfig.ActiveModsInLoadOrder.Count<ModMetaData>() != 1 || !ModsConfig.ActiveModsInLoadOrder.First<ModMetaData>().IsCoreMod)
			{
				text = text + "\n\n" + "ErrorWhileLoadingAssets_ModsInfo".Translate();
			}
			DelayedErrorWindowRequest.Add(text, "ErrorWhileLoadingAssetsTitle".Translate());
			RootMap.GoToMainMenu();
		}

		private static void ErrorWhileGeneratingMap(Exception e)
		{
			DelayedErrorWindowRequest.Add("ErrorWhileGeneratingMap".Translate(), "ErrorWhileGeneratingMapTitle".Translate());
			CrossRefResolver.Clear();
			PostLoadInitter.Clear();
			RootMap.GoToMainMenu();
		}

		private static void ErrorWhileLoadingMap(Exception e)
		{
			string text = "ErrorWhileLoadingMap".Translate();
			string text2;
			string text3;
			if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out text2, out text3))
			{
				text = text + "\n\n" + "ModsMismatchWarningText".Translate(new object[]
				{
					text2,
					text3
				});
			}
			DelayedErrorWindowRequest.Add(text, "ErrorWhileLoadingMapTitle".Translate());
			CrossRefResolver.Clear();
			PostLoadInitter.Clear();
			RootMap.GoToMainMenu();
		}
	}
}
