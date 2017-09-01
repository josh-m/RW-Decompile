using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

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
				MapComponentUtility.MapComponentOnGUI(Find.VisibleMap);
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
				if (DebugViewSettings.drawAttackTargetScores)
				{
					AttackTargetFinder.DebugDrawAttackTargetScores_OnGUI();
				}
				if (!screenshotMode.FiltersCurrentEvent)
				{
					this.globalControls.GlobalControlsOnGUI();
					this.resourceReadout.ResourceReadoutOnGUI();
				}
			}
			else
			{
				this.targeter.StopTargeting();
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
				Find.VisibleMap.deepResourceGrid.DeepResourceGridUpdate();
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
				if (DebugViewSettings.drawAttackTargetScores)
				{
					AttackTargetFinder.DebugDrawAttackTargetScores_Update();
				}
				MiscDebugDrawer.DebugDrawInteractionCells();
				Find.VisibleMap.debugDrawer.DebugDrawerUpdate();
				Find.VisibleMap.regionGrid.DebugDraw();
				InfestationCellFinder.DebugDraw();
				StealAIDebugDrawer.DebugDraw();
			}
		}

		public void Notify_SwitchedMap()
		{
			this.designatorManager.Deselect();
			this.reverseDesignatorDatabase.Reinit();
			this.selector.ClearSelection();
			this.selector.dragBox.active = false;
			this.targeter.StopTargeting();
			MainButtonDef openTab = Find.MainTabsRoot.OpenTab;
			List<MainButtonDef> allDefsListForReading = DefDatabase<MainButtonDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				allDefsListForReading[i].Notify_SwitchedMap();
			}
			if (openTab != null && openTab != MainButtonDefOf.Inspect)
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
