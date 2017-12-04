using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldRoutePlanner
	{
		private bool active;

		private List<Pawn> caravanPawnsFromFormCaravanDialog = new List<Pawn>();

		private Dialog_FormCaravan currentFormCaravanDialog;

		private List<WorldPath> paths = new List<WorldPath>();

		private List<int> cachedTicksToWaypoint = new List<int>();

		public List<RoutePlannerWaypoint> waypoints = new List<RoutePlannerWaypoint>();

		private bool cantRemoveFirstWaypoint;

		private const int MaxCount = 25;

		private static readonly Texture2D ButtonTex = ContentFinder<Texture2D>.Get("UI/Misc/WorldRoutePlanner", true);

		private static readonly Texture2D MouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/WaypointMouseAttachment", true);

		private static readonly Vector2 BottomWindowSize = new Vector2(500f, 95f);

		private const float BottomWindowBotMargin = 45f;

		private const float BottomWindowEntryExtraBotMargin = 22f;

		private const int DefaultCaravanTicksPerMove = 100;

		public bool Active
		{
			get
			{
				return this.active;
			}
		}

		private bool ShouldStop
		{
			get
			{
				return !this.active || !WorldRendererUtility.WorldRenderedNow || (Current.ProgramState == ProgramState.Playing && Find.TickManager.CurTimeSpeed != TimeSpeed.Paused);
			}
		}

		private int CaravanTicksPerMove
		{
			get
			{
				List<Pawn> caravanPawns = this.CaravanPawns;
				if (!caravanPawns.NullOrEmpty<Pawn>())
				{
					return CaravanTicksPerMoveUtility.GetTicksPerMove(caravanPawns);
				}
				return 3000;
			}
		}

		private List<Pawn> CaravanPawns
		{
			get
			{
				if (this.currentFormCaravanDialog != null)
				{
					return this.caravanPawnsFromFormCaravanDialog;
				}
				Caravan caravanAtTheFirstWaypoint = this.CaravanAtTheFirstWaypoint;
				if (caravanAtTheFirstWaypoint != null)
				{
					return caravanAtTheFirstWaypoint.PawnsListForReading;
				}
				return null;
			}
		}

		private Caravan CaravanAtTheFirstWaypoint
		{
			get
			{
				if (!this.waypoints.Any<RoutePlannerWaypoint>())
				{
					return null;
				}
				return Find.WorldObjects.PlayerControlledCaravanAt(this.waypoints[0].Tile);
			}
		}

		public void Start()
		{
			if (this.active)
			{
				this.Stop();
			}
			this.active = true;
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.Planet;
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			}
		}

		public void Start(Dialog_FormCaravan formCaravanDialog)
		{
			if (this.active)
			{
				this.Stop();
			}
			this.currentFormCaravanDialog = formCaravanDialog;
			this.caravanPawnsFromFormCaravanDialog.AddRange(TransferableUtility.GetPawnsFromTransferables(formCaravanDialog.transferables));
			Find.WindowStack.TryRemove(formCaravanDialog, true);
			this.Start();
			this.TryAddWaypoint(formCaravanDialog.CurrentTile, true);
			this.cantRemoveFirstWaypoint = true;
		}

		public void Stop()
		{
			this.active = false;
			WorldObjectsHolder worldObjects = Find.WorldObjects;
			for (int i = 0; i < this.waypoints.Count; i++)
			{
				worldObjects.Remove(this.waypoints[i]);
			}
			this.waypoints.Clear();
			this.cachedTicksToWaypoint.Clear();
			this.caravanPawnsFromFormCaravanDialog.Clear();
			this.currentFormCaravanDialog = null;
			this.cantRemoveFirstWaypoint = false;
			this.ReleasePaths();
		}

		public void WorldRoutePlannerUpdate()
		{
			if (this.active && this.ShouldStop)
			{
				this.Stop();
			}
			if (!this.active)
			{
				return;
			}
			for (int i = 0; i < this.paths.Count; i++)
			{
				this.paths[i].DrawPath(null);
			}
		}

		public void WorldRoutePlannerOnGUI()
		{
			WorldRoutePlanner.<WorldRoutePlannerOnGUI>c__AnonStorey2 <WorldRoutePlannerOnGUI>c__AnonStorey = new WorldRoutePlanner.<WorldRoutePlannerOnGUI>c__AnonStorey2();
			<WorldRoutePlannerOnGUI>c__AnonStorey.$this = this;
			if (!this.active)
			{
				return;
			}
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				if (this.currentFormCaravanDialog != null)
				{
					Find.WindowStack.Add(this.currentFormCaravanDialog);
				}
				else
				{
					SoundDefOf.TickLow.PlayOneShotOnCamera(null);
				}
				this.Stop();
				Event.current.Use();
				return;
			}
			GenUI.DrawMouseAttachment(WorldRoutePlanner.MouseAttachment);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
			{
				Caravan caravan = Find.WorldSelector.SelectableObjectsUnderMouse().FirstOrDefault<WorldObject>() as Caravan;
				int tile = (caravan == null) ? GenWorld.MouseTile(true) : caravan.Tile;
				if (tile >= 0)
				{
					RoutePlannerWaypoint waypoint = this.MostRecentWaypointAt(tile);
					if (waypoint != null)
					{
						if (waypoint == this.waypoints[this.waypoints.Count - 1])
						{
							this.TryRemoveWaypoint(waypoint, true);
						}
						else
						{
							List<FloatMenuOption> list = new List<FloatMenuOption>();
							list.Add(new FloatMenuOption("AddWaypoint".Translate(), delegate
							{
								<WorldRoutePlannerOnGUI>c__AnonStorey.$this.TryAddWaypoint(tile, true);
							}, MenuOptionPriority.Default, null, null, 0f, null, null));
							list.Add(new FloatMenuOption("RemoveWaypoint".Translate(), delegate
							{
								<WorldRoutePlannerOnGUI>c__AnonStorey.$this.TryRemoveWaypoint(waypoint, true);
							}, MenuOptionPriority.Default, null, null, 0f, null, null));
							Find.WindowStack.Add(new FloatMenu(list));
						}
					}
					else
					{
						this.TryAddWaypoint(tile, true);
					}
					Event.current.Use();
				}
			}
			<WorldRoutePlannerOnGUI>c__AnonStorey.rect = new Rect(((float)UI.screenWidth - WorldRoutePlanner.BottomWindowSize.x) / 2f, (float)UI.screenHeight - WorldRoutePlanner.BottomWindowSize.y - 45f, WorldRoutePlanner.BottomWindowSize.x, WorldRoutePlanner.BottomWindowSize.y);
			if (Current.ProgramState == ProgramState.Entry)
			{
				WorldRoutePlanner.<WorldRoutePlannerOnGUI>c__AnonStorey2 expr_242_cp_0 = <WorldRoutePlannerOnGUI>c__AnonStorey;
				expr_242_cp_0.rect.y = expr_242_cp_0.rect.y - 22f;
			}
			Find.WindowStack.ImmediateWindow(1373514241, <WorldRoutePlannerOnGUI>c__AnonStorey.rect, WindowLayer.Dialog, delegate
			{
				if (!<WorldRoutePlannerOnGUI>c__AnonStorey.$this.active)
				{
					return;
				}
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Small;
				float num = 6f;
				if (<WorldRoutePlannerOnGUI>c__AnonStorey.$this.waypoints.Count >= 2)
				{
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerEstTimeToFinalDest".Translate(new object[]
					{
						<WorldRoutePlannerOnGUI>c__AnonStorey.$this.GetTicksToWaypoint(<WorldRoutePlannerOnGUI>c__AnonStorey.$this.waypoints.Count - 1).ToStringTicksToDays("0.#")
					}));
				}
				else if (<WorldRoutePlannerOnGUI>c__AnonStorey.$this.cantRemoveFirstWaypoint)
				{
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerAddOneOrMoreWaypoints".Translate());
				}
				else
				{
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerAddTwoOrMoreWaypoints".Translate());
				}
				num += 20f;
				if (<WorldRoutePlannerOnGUI>c__AnonStorey.$this.CaravanPawns.NullOrEmpty<Pawn>())
				{
					GUI.color = new Color(0.8f, 0.6f, 0.6f);
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerUsingAverageTicksPerMoveWarning".Translate());
				}
				else if (<WorldRoutePlannerOnGUI>c__AnonStorey.$this.currentFormCaravanDialog == null && <WorldRoutePlannerOnGUI>c__AnonStorey.$this.CaravanAtTheFirstWaypoint != null)
				{
					GUI.color = Color.gray;
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerUsingTicksPerMoveOfCaravan".Translate(new object[]
					{
						<WorldRoutePlannerOnGUI>c__AnonStorey.$this.CaravanAtTheFirstWaypoint.LabelCap
					}));
				}
				num += 20f;
				GUI.color = Color.gray;
				Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerPressRMBToAddAndRemoveWaypoints".Translate());
				num += 20f;
				if (<WorldRoutePlannerOnGUI>c__AnonStorey.$this.currentFormCaravanDialog != null)
				{
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerPressEscapeToReturnToCaravanFormationDialog".Translate());
				}
				else
				{
					Widgets.Label(new Rect(0f, num, <WorldRoutePlannerOnGUI>c__AnonStorey.rect.width, 25f), "RoutePlannerPressEscapeToExit".Translate());
				}
				num += 20f;
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}, true, false, 1f);
		}

		public void DoRoutePlannerButton(ref float curBaseY)
		{
			float num = (float)WorldRoutePlanner.ButtonTex.width;
			float num2 = (float)WorldRoutePlanner.ButtonTex.height;
			Rect rect = new Rect((float)UI.screenWidth - 10f - num, curBaseY - 10f - num2, num, num2);
			if (Widgets.ButtonImage(rect, WorldRoutePlanner.ButtonTex, Color.white, new Color(0.8f, 0.8f, 0.8f)))
			{
				if (this.active)
				{
					this.Stop();
					SoundDefOf.TickLow.PlayOneShotOnCamera(null);
				}
				else
				{
					this.Start();
					SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
				}
			}
			TooltipHandler.TipRegion(rect, "RoutePlannerButtonTip".Translate());
			curBaseY -= num2 + 20f;
		}

		public int GetTicksToWaypoint(int index)
		{
			return this.cachedTicksToWaypoint[index];
		}

		private void TryAddWaypoint(int tile, bool playSound = true)
		{
			if (Find.World.Impassable(tile))
			{
				Messages.Message("MessageCantAddWaypointBecauseImpassable".Translate(), MessageTypeDefOf.RejectInput);
				return;
			}
			if (this.waypoints.Any<RoutePlannerWaypoint>() && !Find.WorldReachability.CanReach(this.waypoints[this.waypoints.Count - 1].Tile, tile))
			{
				Messages.Message("MessageCantAddWaypointBecauseUnreachable".Translate(), MessageTypeDefOf.RejectInput);
				return;
			}
			if (this.waypoints.Count >= 25)
			{
				Messages.Message("MessageCantAddWaypointBecauseLimit".Translate(new object[]
				{
					25
				}), MessageTypeDefOf.RejectInput);
				return;
			}
			RoutePlannerWaypoint routePlannerWaypoint = (RoutePlannerWaypoint)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.RoutePlannerWaypoint);
			routePlannerWaypoint.Tile = tile;
			Find.WorldObjects.Add(routePlannerWaypoint);
			this.waypoints.Add(routePlannerWaypoint);
			this.RecreatePaths();
			if (playSound)
			{
				SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
			}
		}

		public void TryRemoveWaypoint(RoutePlannerWaypoint point, bool playSound = true)
		{
			if (this.cantRemoveFirstWaypoint && this.waypoints.Any<RoutePlannerWaypoint>() && point == this.waypoints[0])
			{
				Messages.Message("MessageCantRemoveWaypointBecauseFirst".Translate(), MessageTypeDefOf.RejectInput);
				return;
			}
			Find.WorldObjects.Remove(point);
			this.waypoints.Remove(point);
			for (int i = this.waypoints.Count - 1; i >= 1; i--)
			{
				if (this.waypoints[i].Tile == this.waypoints[i - 1].Tile)
				{
					Find.WorldObjects.Remove(this.waypoints[i]);
					this.waypoints.RemoveAt(i);
				}
			}
			this.RecreatePaths();
			if (playSound)
			{
				SoundDefOf.TickLow.PlayOneShotOnCamera(null);
			}
		}

		private void ReleasePaths()
		{
			for (int i = 0; i < this.paths.Count; i++)
			{
				this.paths[i].ReleaseToPool();
			}
			this.paths.Clear();
		}

		private void RecreatePaths()
		{
			this.ReleasePaths();
			WorldPathFinder worldPathFinder = Find.WorldPathFinder;
			for (int i = 1; i < this.waypoints.Count; i++)
			{
				this.paths.Add(worldPathFinder.FindPath(this.waypoints[i - 1].Tile, this.waypoints[i].Tile, null, null));
			}
			this.cachedTicksToWaypoint.Clear();
			int num = 0;
			int caravanTicksPerMove = this.CaravanTicksPerMove;
			for (int j = 0; j < this.waypoints.Count; j++)
			{
				if (j == 0)
				{
					this.cachedTicksToWaypoint.Add(0);
				}
				else
				{
					num += CaravanArrivalTimeEstimator.EstimatedTicksToArrive(this.waypoints[j - 1].Tile, this.waypoints[j].Tile, this.paths[j - 1], 0f, caravanTicksPerMove, GenTicks.TicksAbs + num);
					this.cachedTicksToWaypoint.Add(num);
				}
			}
		}

		private RoutePlannerWaypoint MostRecentWaypointAt(int tile)
		{
			for (int i = this.waypoints.Count - 1; i >= 0; i--)
			{
				if (this.waypoints[i].Tile == tile)
				{
					return this.waypoints[i];
				}
			}
			return null;
		}
	}
}
