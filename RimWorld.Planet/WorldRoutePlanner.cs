using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldRoutePlanner
	{
		private bool active;

		private CaravanTicksPerMoveUtility.CaravanInfo? caravanInfoFromFormCaravanDialog;

		private Dialog_FormCaravan currentFormCaravanDialog;

		private List<WorldPath> paths = new List<WorldPath>();

		private List<int> cachedTicksToWaypoint = new List<int>();

		public List<RoutePlannerWaypoint> waypoints = new List<RoutePlannerWaypoint>();

		private bool cantRemoveFirstWaypoint;

		private const int MaxCount = 25;

		private static readonly Texture2D ButtonTex = ContentFinder<Texture2D>.Get("UI/Misc/WorldRoutePlanner", true);

		private static readonly Texture2D MouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/WaypointMouseAttachment", true);

		private static readonly Vector2 BottomWindowSize = new Vector2(500f, 95f);

		private static readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		private const float BottomWindowBotMargin = 45f;

		private const float BottomWindowEntryExtraBotMargin = 22f;

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
				CaravanTicksPerMoveUtility.CaravanInfo? caravanInfo = this.CaravanInfo;
				if (caravanInfo.HasValue && caravanInfo.Value.pawns.Any<Pawn>())
				{
					return CaravanTicksPerMoveUtility.GetTicksPerMove(caravanInfo.Value, null);
				}
				return 3464;
			}
		}

		private CaravanTicksPerMoveUtility.CaravanInfo? CaravanInfo
		{
			get
			{
				if (this.currentFormCaravanDialog != null)
				{
					return this.caravanInfoFromFormCaravanDialog;
				}
				Caravan caravanAtTheFirstWaypoint = this.CaravanAtTheFirstWaypoint;
				if (caravanAtTheFirstWaypoint != null)
				{
					return new CaravanTicksPerMoveUtility.CaravanInfo?(new CaravanTicksPerMoveUtility.CaravanInfo(caravanAtTheFirstWaypoint));
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
				Find.TickManager.Pause();
			}
		}

		public void Start(Dialog_FormCaravan formCaravanDialog)
		{
			if (this.active)
			{
				this.Stop();
			}
			this.currentFormCaravanDialog = formCaravanDialog;
			this.caravanInfoFromFormCaravanDialog = new CaravanTicksPerMoveUtility.CaravanInfo?(new CaravanTicksPerMoveUtility.CaravanInfo(formCaravanDialog));
			formCaravanDialog.choosingRoute = true;
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
			if (this.currentFormCaravanDialog != null)
			{
				this.currentFormCaravanDialog.Notify_NoLongerChoosingRoute();
			}
			this.caravanInfoFromFormCaravanDialog = null;
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
			if (!this.active)
			{
				return;
			}
			if (KeyBindingDefOf.Cancel.KeyDownEvent)
			{
				if (this.currentFormCaravanDialog != null)
				{
					Find.WindowStack.Add(this.currentFormCaravanDialog);
				}
				else
				{
					SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
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
								this.TryAddWaypoint(tile, true);
							}, MenuOptionPriority.Default, null, null, 0f, null, null));
							list.Add(new FloatMenuOption("RemoveWaypoint".Translate(), delegate
							{
								this.TryRemoveWaypoint(waypoint, true);
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
			this.DoRouteDetailsBox();
			if (this.DoChooseRouteButton())
			{
				return;
			}
			this.DoTileTooltips();
		}

		private void DoRouteDetailsBox()
		{
			WorldRoutePlanner.<DoRouteDetailsBox>c__AnonStorey2 <DoRouteDetailsBox>c__AnonStorey = new WorldRoutePlanner.<DoRouteDetailsBox>c__AnonStorey2();
			<DoRouteDetailsBox>c__AnonStorey.$this = this;
			<DoRouteDetailsBox>c__AnonStorey.rect = new Rect(((float)UI.screenWidth - WorldRoutePlanner.BottomWindowSize.x) / 2f, (float)UI.screenHeight - WorldRoutePlanner.BottomWindowSize.y - 45f, WorldRoutePlanner.BottomWindowSize.x, WorldRoutePlanner.BottomWindowSize.y);
			if (Current.ProgramState == ProgramState.Entry)
			{
				WorldRoutePlanner.<DoRouteDetailsBox>c__AnonStorey2 expr_77_cp_0 = <DoRouteDetailsBox>c__AnonStorey;
				expr_77_cp_0.rect.y = expr_77_cp_0.rect.y - 22f;
			}
			Find.WindowStack.ImmediateWindow(1373514241, <DoRouteDetailsBox>c__AnonStorey.rect, WindowLayer.Dialog, delegate
			{
				if (!<DoRouteDetailsBox>c__AnonStorey.$this.active)
				{
					return;
				}
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Small;
				float num = 6f;
				if (<DoRouteDetailsBox>c__AnonStorey.$this.waypoints.Count >= 2)
				{
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerEstTimeToFinalDest".Translate(new object[]
					{
						<DoRouteDetailsBox>c__AnonStorey.$this.GetTicksToWaypoint(<DoRouteDetailsBox>c__AnonStorey.$this.waypoints.Count - 1).ToStringTicksToDays("0.#")
					}));
				}
				else if (<DoRouteDetailsBox>c__AnonStorey.$this.cantRemoveFirstWaypoint)
				{
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerAddOneOrMoreWaypoints".Translate());
				}
				else
				{
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerAddTwoOrMoreWaypoints".Translate());
				}
				num += 20f;
				if (!<DoRouteDetailsBox>c__AnonStorey.$this.CaravanInfo.HasValue || !<DoRouteDetailsBox>c__AnonStorey.$this.CaravanInfo.Value.pawns.Any<Pawn>())
				{
					GUI.color = new Color(0.8f, 0.6f, 0.6f);
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerUsingAverageTicksPerMoveWarning".Translate());
				}
				else if (<DoRouteDetailsBox>c__AnonStorey.$this.currentFormCaravanDialog == null && <DoRouteDetailsBox>c__AnonStorey.$this.CaravanAtTheFirstWaypoint != null)
				{
					GUI.color = Color.gray;
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerUsingTicksPerMoveOfCaravan".Translate(new object[]
					{
						<DoRouteDetailsBox>c__AnonStorey.$this.CaravanAtTheFirstWaypoint.LabelCap
					}));
				}
				num += 20f;
				GUI.color = Color.gray;
				Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerPressRMBToAddAndRemoveWaypoints".Translate());
				num += 20f;
				if (<DoRouteDetailsBox>c__AnonStorey.$this.currentFormCaravanDialog != null)
				{
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerPressEscapeToReturnToCaravanFormationDialog".Translate());
				}
				else
				{
					Widgets.Label(new Rect(0f, num, <DoRouteDetailsBox>c__AnonStorey.rect.width, 25f), "RoutePlannerPressEscapeToExit".Translate());
				}
				num += 20f;
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}, true, false, 1f);
		}

		private bool DoChooseRouteButton()
		{
			if (this.currentFormCaravanDialog == null || this.waypoints.Count < 2)
			{
				return false;
			}
			Rect rect = new Rect(((float)UI.screenWidth - WorldRoutePlanner.BottomButtonSize.x) / 2f, (float)UI.screenHeight - WorldRoutePlanner.BottomWindowSize.y - 45f - 10f - WorldRoutePlanner.BottomButtonSize.y, WorldRoutePlanner.BottomButtonSize.x, WorldRoutePlanner.BottomButtonSize.y);
			if (Widgets.ButtonText(rect, "ChooseRouteButton".Translate(), true, false, true))
			{
				Find.WindowStack.Add(this.currentFormCaravanDialog);
				this.currentFormCaravanDialog.Notify_ChoseRoute(this.waypoints[1].Tile);
				this.Stop();
				return true;
			}
			return false;
		}

		private void DoTileTooltips()
		{
			if (Mouse.IsInputBlockedNow)
			{
				return;
			}
			int num = GenWorld.MouseTile(true);
			if (num == -1)
			{
				return;
			}
			for (int i = 0; i < this.paths.Count; i++)
			{
				if (this.paths[i].NodesReversed.Contains(num))
				{
					string str = this.GetTileTip(num, i);
					Text.Font = GameFont.Small;
					Vector2 size = Text.CalcSize(str);
					size.x += 20f;
					size.y += 20f;
					Vector2 mouseAttachedWindowPos = GenUI.GetMouseAttachedWindowPos(size.x, size.y);
					Rect rect = new Rect(mouseAttachedWindowPos, size);
					Find.WindowStack.ImmediateWindow(1859615246, rect, WindowLayer.Super, delegate
					{
						Text.Font = GameFont.Small;
						Rect rect = rect.AtZero().ContractedBy(10f);
						Widgets.Label(rect, str);
					}, true, false, 1f);
					break;
				}
			}
		}

		private string GetTileTip(int tile, int pathIndex)
		{
			int num = this.paths[pathIndex].NodesReversed.IndexOf(tile);
			int num2;
			if (num > 0)
			{
				num2 = this.paths[pathIndex].NodesReversed[num - 1];
			}
			else if (pathIndex < this.paths.Count - 1 && this.paths[pathIndex + 1].NodesReversed.Count >= 2)
			{
				num2 = this.paths[pathIndex + 1].NodesReversed[this.paths[pathIndex + 1].NodesReversed.Count - 2];
			}
			else
			{
				num2 = -1;
			}
			int num3 = this.cachedTicksToWaypoint[pathIndex] + CaravanArrivalTimeEstimator.EstimatedTicksToArrive(this.paths[pathIndex].FirstNode, tile, this.paths[pathIndex], 0f, this.CaravanTicksPerMove, GenTicks.TicksAbs + this.cachedTicksToWaypoint[pathIndex]);
			int num4 = GenTicks.TicksAbs + num3;
			StringBuilder stringBuilder = new StringBuilder();
			if (num3 != 0)
			{
				stringBuilder.AppendLine("EstimatedTimeToTile".Translate(new object[]
				{
					num3.ToStringTicksToDays("0.##")
				}));
			}
			stringBuilder.AppendLine("ForagedFoodAmount".Translate() + ": " + Find.WorldGrid[tile].biome.forageability.ToStringPercent());
			stringBuilder.Append(VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(tile, num4));
			if (num2 != -1)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				StringBuilder stringBuilder2 = new StringBuilder();
				float num5 = WorldPathGrid.CalculatedMovementDifficultyAt(num2, false, new int?(num4), stringBuilder2);
				float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(tile, num2, stringBuilder2);
				stringBuilder.Append("TileMovementDifficulty".Translate() + ":\n" + stringBuilder2.ToString().Indented("  "));
				stringBuilder.AppendLine();
				stringBuilder.Append("  = ");
				stringBuilder.Append((num5 * roadMovementDifficultyMultiplier).ToString("0.#"));
			}
			return stringBuilder.ToString();
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
					SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
				}
				else
				{
					this.Start();
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
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
				Messages.Message("MessageCantAddWaypointBecauseImpassable".Translate(), MessageTypeDefOf.RejectInput, false);
				return;
			}
			if (this.waypoints.Any<RoutePlannerWaypoint>() && !Find.WorldReachability.CanReach(this.waypoints[this.waypoints.Count - 1].Tile, tile))
			{
				Messages.Message("MessageCantAddWaypointBecauseUnreachable".Translate(), MessageTypeDefOf.RejectInput, false);
				return;
			}
			if (this.waypoints.Count >= 25)
			{
				Messages.Message("MessageCantAddWaypointBecauseLimit".Translate(new object[]
				{
					25
				}), MessageTypeDefOf.RejectInput, false);
				return;
			}
			RoutePlannerWaypoint routePlannerWaypoint = (RoutePlannerWaypoint)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.RoutePlannerWaypoint);
			routePlannerWaypoint.Tile = tile;
			Find.WorldObjects.Add(routePlannerWaypoint);
			this.waypoints.Add(routePlannerWaypoint);
			this.RecreatePaths();
			if (playSound)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
			}
		}

		public void TryRemoveWaypoint(RoutePlannerWaypoint point, bool playSound = true)
		{
			if (this.cantRemoveFirstWaypoint && this.waypoints.Any<RoutePlannerWaypoint>() && point == this.waypoints[0])
			{
				Messages.Message("MessageCantRemoveWaypointBecauseFirst".Translate(), MessageTypeDefOf.RejectInput, false);
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
				SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
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
