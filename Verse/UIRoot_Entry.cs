using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public class UIRoot_Entry : UIRoot
	{
		private bool ShouldDoMainMenu
		{
			get
			{
				for (int i = 0; i < Find.WindowStack.Count; i++)
				{
					if (this.windows[i].layer == WindowLayer.Dialog && !Find.WindowStack[i].IsDebug)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override void Init()
		{
			base.Init();
			UIMenuBackgroundManager.background = new UI_BackgroundMain();
			MainMenuDrawer.Init();
			QuickStarter.CheckQuickStart();
			VersionUpdateDialogMaker.CreateVersionUpdateDialogIfNecessary();
			if (!SteamManager.Initialized)
			{
				string text = "SteamClientMissing".Translate();
				Dialog_MessageBox window = new Dialog_MessageBox(text, "Quit".Translate(), delegate
				{
					Application.Quit();
				}, "Ignore".Translate(), null, null, false);
				Find.WindowStack.Add(window);
			}
		}

		public override void UIRootOnGUI()
		{
			base.UIRootOnGUI();
			if (Find.World != null)
			{
				Find.World.UI.WorldInterfaceOnGUI();
			}
			this.DoMainMenu();
			if (Current.Game != null)
			{
				Find.Tutor.TutorOnGUI();
			}
			this.windows.WindowStackOnGUI();
			ReorderableWidget.ReorderableWidgetOnGUI();
			if (Find.World != null)
			{
				Find.World.UI.HandleLowPriorityInput();
			}
		}

		public override void UIRootUpdate()
		{
			base.UIRootUpdate();
			if (Find.World != null)
			{
				Find.World.UI.WorldInterfaceUpdate();
			}
			if (Current.Game != null)
			{
				LessonAutoActivator.LessonAutoActivatorUpdate();
				Find.Tutor.TutorUpdate();
			}
		}

		private void DoMainMenu()
		{
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				UIMenuBackgroundManager.background.BackgroundOnGUI();
				if (this.ShouldDoMainMenu)
				{
					Current.Game = null;
					MainMenuDrawer.MainMenuOnGUI();
				}
			}
		}
	}
}
