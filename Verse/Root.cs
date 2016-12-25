using RimWorld;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse
{
	public abstract class Root : MonoBehaviour
	{
		private static bool globalInitDone;

		private static bool prefsApplied;

		protected bool destroyed;

		public SoundRoot soundRoot;

		public UIRoot uiRoot;

		public virtual void Start()
		{
			Current.Notify_LoadedSceneChanged();
			Root.CheckGlobalInit();
			Action action = delegate
			{
				this.soundRoot = new SoundRoot();
				if (GenScene.InPlayScene)
				{
					this.uiRoot = new UIRoot_Play();
				}
				else if (GenScene.InEntryScene)
				{
					this.uiRoot = new UIRoot_Entry();
				}
				this.uiRoot.Init();
				Messages.Notify_LoadedLevelChanged();
			};
			if (!PlayDataLoader.Loaded)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					PlayDataLoader.LoadAllPlayData(false);
				}, null, true, null);
				LongEventHandler.QueueLongEvent(action, "InitializingInterface", false, null);
			}
			else
			{
				action();
			}
		}

		private static void CheckGlobalInit()
		{
			if (Root.globalInitDone)
			{
				return;
			}
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			if (currentCulture.Name != "en-US")
			{
				Log.Warning("Unexpected culture: " + currentCulture + ". Resetting to en-US.");
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			}
			SteamManager.InitIfNeeded();
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (commandLineArgs != null && commandLineArgs.Length > 1)
			{
				Log.Message("Command line arguments: " + GenText.ToSpaceList(commandLineArgs.Skip(1)));
			}
			UnityData.CopyUnityData();
			VersionControl.LogVersionNumber();
			Application.targetFrameRate = 60;
			Prefs.Init();
			if (Prefs.DevMode)
			{
				StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes();
			}
			LongEventHandler.QueueLongEvent(new Action(StaticConstructorOnStartupUtility.CallAll), null, false, null);
			Root.globalInitDone = true;
		}

		public virtual void Update()
		{
			try
			{
				RealTime.Update();
				bool flag;
				LongEventHandler.LongEventsUpdate(out flag);
				if (flag)
				{
					this.destroyed = true;
				}
				else if (!LongEventHandler.ShouldWaitForEvent)
				{
					Rand.EnsureSeedStackEmpty();
					SteamManager.Update();
					PortraitsCache.PortraitsCacheUpdate();
					this.uiRoot.UIRootUpdate();
					if (Time.frameCount > 3 && !Root.prefsApplied)
					{
						Root.prefsApplied = true;
						Prefs.Apply();
					}
					this.soundRoot.Update();
				}
			}
			catch (Exception e)
			{
				Log.Notify_Exception(e);
				throw;
			}
		}

		public void OnGUI()
		{
			UI.ApplyPixelScale();
			try
			{
				if (!this.destroyed)
				{
					LongEventHandler.LongEventsOnGUI();
					if (LongEventHandler.ShouldWaitForEvent)
					{
						ScreenFader.OverlayOnGUI(new Vector2((float)UI.screenWidth, (float)UI.screenHeight));
					}
					else
					{
						this.uiRoot.UIRootOnGUI();
						ScreenFader.OverlayOnGUI(new Vector2((float)UI.screenWidth, (float)UI.screenHeight));
					}
				}
			}
			catch (Exception e)
			{
				Log.Notify_Exception(e);
				throw;
			}
		}

		public static void Shutdown()
		{
			SteamManager.ShutdownSteam();
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.TempFolderPath);
			FileInfo[] files = directoryInfo.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				fileInfo.Delete();
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryInfo directoryInfo2 = directories[j];
				directoryInfo2.Delete(true);
			}
			Application.Quit();
		}
	}
}
