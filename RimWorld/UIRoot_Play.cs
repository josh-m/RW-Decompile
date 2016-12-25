using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class UIRoot_Play : UIRoot
	{
		public MapInterface mapUI = new MapInterface();

		public MainTabsRoot mainTabsRoot = new MainTabsRoot();

		public AlertsReadout alerts = new AlertsReadout();

		public override void Init()
		{
			base.Init();
			Messages.Clear();
		}

		public override void UIRootOnGUI()
		{
			base.UIRootOnGUI();
			Find.GameInfo.GameInfoOnGUI();
			Find.World.UI.WorldInterfaceOnGUI();
			this.mapUI.MapInterfaceOnGUI_BeforeMainTabs();
			if (!this.screenshotMode.FiltersCurrentEvent)
			{
				this.mainTabsRoot.MainTabsOnGUI();
				this.alerts.AlertsReadoutOnGUI();
			}
			this.mapUI.MapInterfaceOnGUI_AfterMainTabs();
			if (!this.screenshotMode.FiltersCurrentEvent)
			{
				Find.Tutor.TutorOnGUI();
			}
			this.windows.WindowStackOnGUI();
			ReorderableWidget.ReorderableWidgetOnGUI();
			this.mapUI.HandleMapClicks();
			DebugTools.DebugToolsOnGUI();
			this.mainTabsRoot.HandleLowPriorityShortcuts();
			Find.World.UI.HandleLowPriorityInput();
			this.mapUI.HandleLowPriorityInput();
			this.OpenMainMenuShortcut();
		}

		public override void UIRootUpdate()
		{
			base.UIRootUpdate();
			try
			{
				Find.World.UI.WorldInterfaceUpdate();
				this.mapUI.MapInterfaceUpdate();
				this.alerts.AlertsReadoutUpdate();
				LessonAutoActivator.LessonAutoActivatorUpdate();
				Find.Tutor.TutorUpdate();
			}
			catch (Exception ex)
			{
				Log.Error("Exception in UIRootUpdate: " + ex.ToString());
			}
		}

		private void OpenMainMenuShortcut()
		{
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				Event.current.Use();
				this.mainTabsRoot.SetCurrentTab(MainTabDefOf.Menu, true);
			}
		}
	}
}
