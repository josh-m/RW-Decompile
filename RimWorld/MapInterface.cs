using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class MapInterface
	{
		public ThingOverlays thingOverlays = new ThingOverlays();

		public Selector selector = new Selector();

		public Targeter targeter = new Targeter();

		public DesignatorManager designatorManager = new DesignatorManager();

		public ReverseDesignatorDatabase reverseDesignatorDatabase = new ReverseDesignatorDatabase();

		private MouseoverReadout mouseoverReadout = new MouseoverReadout();

		public GlobalControls globalControls = new GlobalControls();

		protected ResourceReadout resourceReadout = new ResourceReadout();

		public ColonistBar colonistBar = new ColonistBar();

		public void MapInterfaceOnGUI_BeforeMainTabs()
		{
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
				this.thingOverlays.ThingOverlaysOnGUI();
				for (int i = 0; i < Find.VisibleMap.components.Count; i++)
				{
					Find.VisibleMap.components[i].MapComponentOnGUI();
				}
				if (!screenshotMode.FiltersCurrentEvent)
				{
					this.colonistBar.ColonistBarOnGUI();
				}
				this.selector.dragBox.DragBoxOnGUI();
				this.designatorManager.DesignationManagerOnGUI();
				this.targeter.TargeterOnGUI();
				Find.VisibleMap.tooltipGiverList.DispenseAllThingTooltips();
				if (DebugViewSettings.drawFoodSearchFromMouse)
				{
					FoodUtility.DebugFoodSearchFromMouse_OnGUI();
				}
				if (!screenshotMode.FiltersCurrentEvent)
				{
					this.globalControls.GlobalControlsOnGUI();
					this.resourceReadout.ResourceReadoutOnGUI();
				}
			}
		}

		public void MapInterfaceOnGUI_AfterMainTabs()
		{
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
				if (!screenshotMode.FiltersCurrentEvent)
				{
					this.mouseoverReadout.MouseoverReadoutOnGUI();
					EnvironmentInspectDrawer.EnvironmentInspectOnGUI();
					Find.VisibleMap.debugDrawer.DebugDrawerOnGUI();
				}
			}
		}

		public void HandleMapClicks()
		{
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				this.designatorManager.ProcessInputEvents();
				this.targeter.ProcessInputEvents();
			}
		}

		public void HandleLowPriorityInput()
		{
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				this.selector.SelectorOnGUI();
				Find.VisibleMap.lordManager.LordManagerOnGUI();
			}
		}

		public void MapInterfaceUpdate()
		{
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				this.targeter.TargeterUpdate();
				SelectionDrawer.DrawSelectionOverlays();
				EnvironmentInspectDrawer.DrawRoomOverlays();
				this.designatorManager.DesignatorManagerUpdate();
				Find.VisibleMap.roofGrid.RoofGridUpdate();
				Find.VisibleMap.exitMapGrid.ExitMapGridUpdate();
				if (DebugViewSettings.drawPawnDebug)
				{
					Find.VisibleMap.pawnDestinationManager.DebugDrawDestinations();
					Find.VisibleMap.reservationManager.DebugDrawReservations();
				}
				if (DebugViewSettings.drawFoodSearchFromMouse)
				{
					FoodUtility.DebugFoodSearchFromMouse_Update();
				}
				if (DebugViewSettings.drawPreyInfo)
				{
					FoodUtility.DebugDrawPredatorFoodSource();
				}
				Find.VisibleMap.debugDrawer.DebugDrawerUpdate();
				Find.VisibleMap.regionGrid.DebugDraw();
				InfestationCellFinder.DebugDraw();
				StealAIDebugDrawer.DebugDraw();
			}
		}

		public void Notify_SwitchedMap()
		{
			this.designatorManager.ResetSelectedDesignator();
			this.reverseDesignatorDatabase.Reinit();
			this.selector.ClearSelection();
			this.targeter.StopTargeting();
			MainTabDef openTab = Find.MainTabsRoot.OpenTab;
			bool everOpened = ((MainTabWindow_World)MainTabDefOf.World.Window).everOpened;
			List<MainTabDef> allDefsListForReading = DefDatabase<MainTabDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				allDefsListForReading[i].Notify_SwitchedMap();
			}
			((MainTabWindow_World)MainTabDefOf.World.Window).everOpened = everOpened;
			if (openTab != null && openTab != MainTabDefOf.Inspect)
			{
				Find.MainTabsRoot.SetCurrentTab(openTab, false);
			}
			if (Find.VisibleMap != null)
			{
				RememberedCameraPos rememberedCameraPos = Find.VisibleMap.rememberedCameraPos;
				Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
			}
		}
	}
}
