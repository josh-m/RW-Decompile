using RimWorld;
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
				if (Application.isEditor)
				{
					text = "(The below message is for players. In the editor, you can continue without Steam, though anything might break.)\n\n" + text;
				}
				Dialog_Confirm dialog_Confirm = new Dialog_Confirm(text, delegate
				{
					Application.Quit();
				}, false, null, false);
				dialog_Confirm.confirmLabel = "OK".Translate();
				Find.WindowStack.Add(dialog_Confirm);
			}
		}

		public override void UIRootOnGUI()
		{
			base.UIRootOnGUI();
			UIMenuBackgroundManager.background.BackgroundOnGUI();
			if (this.ShouldDoMainMenu)
			{
				Current.Game = null;
				MainMenuDrawer.MainMenuOnGUI();
			}
			if (Current.Game != null)
			{
				Find.Tutor.TutorOnGUI();
			}
			this.windows.WindowStackOnGUI();
			ReorderableWidget.ReorderableWidgetOnGUI();
		}

		public override void UIRootUpdate()
		{
			if (Current.Game != null)
			{
				LessonAutoActivator.LessonAutoActivatorUpdate();
				Find.Tutor.TutorUpdate();
			}
			base.UIRootUpdate();
		}
	}
}
