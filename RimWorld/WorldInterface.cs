using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class WorldInterface
	{
		public WorldSelector selector = new WorldSelector();

		public WorldTargeter targeter = new WorldTargeter();

		public WorldInspectPane inspectPane = new WorldInspectPane();

		public WorldGlobalControls globalControls = new WorldGlobalControls();

		public int SelectedTile
		{
			get
			{
				return this.selector.selectedTile;
			}
			set
			{
				this.selector.selectedTile = value;
			}
		}

		public void Reset()
		{
			this.inspectPane.Reset();
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (Find.VisibleMap != null)
				{
					this.SelectedTile = Find.VisibleMap.Tile;
				}
				else
				{
					this.SelectedTile = -1;
				}
			}
			else if (Find.GameInitData != null)
			{
				if (Find.GameInitData.startingTile >= 0 && Find.World != null && !Find.WorldGrid.InBounds(Find.GameInitData.startingTile))
				{
					Log.Error("Map world tile was out of bounds.");
					Find.GameInitData.startingTile = -1;
				}
				this.SelectedTile = Find.GameInitData.startingTile;
				this.inspectPane.OpenTabType = typeof(WITab_Terrain);
			}
			else
			{
				this.SelectedTile = -1;
			}
			if (this.SelectedTile >= 0)
			{
				Find.WorldCameraDriver.JumpTo(this.SelectedTile);
			}
			else
			{
				Find.WorldCameraDriver.JumpTo(Find.WorldGrid.viewCenter);
			}
			Find.WorldCameraDriver.ResetAltitude();
		}

		public void WorldInterfaceUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			if (worldRenderedNow)
			{
				this.targeter.TargeterUpdate();
				WorldSelectionDrawer.DrawSelectionOverlays();
				Find.WorldDebugDrawer.WorldDebugDrawerUpdate();
			}
		}

		public void WorldInterfaceOnGUI()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			this.CheckOpenOrCloseInspectPane();
			if (worldRenderedNow)
			{
				ScreenshotModeHandler screenshotMode = Find.UIRoot.screenshotMode;
				ExpandableWorldObjectsUtility.ExpandableWorldObjectsOnGUI();
				WorldSelectionDrawer.SelectionOverlaysOnGUI();
				if (!screenshotMode.FiltersCurrentEvent && Current.ProgramState == ProgramState.Playing)
				{
					Find.ColonistBar.ColonistBarOnGUI();
				}
				this.selector.dragBox.DragBoxOnGUI();
				this.targeter.TargeterOnGUI();
				if (!screenshotMode.FiltersCurrentEvent)
				{
					this.globalControls.WorldGlobalControlsOnGUI();
				}
				Find.WorldDebugDrawer.WorldDebugDrawerOnGUI();
			}
		}

		public void HandleLowPriorityInput()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			if (worldRenderedNow)
			{
				this.targeter.ProcessInputEvents();
				this.selector.WorldSelectorOnGUI();
			}
		}

		private void CheckOpenOrCloseInspectPane()
		{
			if (this.selector.AnyObjectOrTileSelected && WorldRendererUtility.WorldRenderedNow && (Current.ProgramState != ProgramState.Playing || Find.MainTabsRoot.OpenTab == MainTabDefOf.World))
			{
				if (!Find.WindowStack.IsOpen<WorldInspectPane>())
				{
					Find.WindowStack.Add(this.inspectPane);
				}
			}
			else if (Find.WindowStack.IsOpen<WorldInspectPane>())
			{
				Find.WindowStack.TryRemove(this.inspectPane, false);
			}
		}
	}
}
