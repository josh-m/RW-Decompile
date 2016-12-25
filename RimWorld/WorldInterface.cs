using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WorldInterface
	{
		private const float MinPixelsToDrag = 5f;

		private const float MouseDragDollySpeed = 0.0015f;

		private const float MouseScrollWheelZoomSpeed = 3.5f;

		private const float KeyZoomSpeed = 20f;

		private const float KeyDollySpeed = 0.1f;

		public IntVec2 selectedCoords;

		private WorldView view;

		private bool viewDragging;

		private Vector2 viewDragStartLoc;

		private static readonly Texture2D SelectedSquareOverlay = ContentFinder<Texture2D>.Get("UI/World/SelectedWorldSquare", true);

		private static readonly Texture2D CurMapSquareOverlay = ContentFinder<Texture2D>.Get("UI/World/MapWorldSquare", true);

		public WorldInterface()
		{
			this.Reset();
		}

		public void Reset()
		{
			this.view = null;
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				this.selectedCoords = Find.Map.WorldCoords;
			}
			else if (Current.Game != null)
			{
				if (Find.GameInitData.startingCoords.IsValid && Find.World != null && !Find.World.InBounds(Find.GameInitData.startingCoords))
				{
					Log.Error("Map world coords were out of bounds.");
					Find.GameInitData.startingCoords = IntVec2.Invalid;
				}
				this.selectedCoords = Find.GameInitData.startingCoords;
			}
			else
			{
				this.selectedCoords = IntVec2.Invalid;
			}
		}

		public void Draw(Rect mainRect, bool selectingLandingSite = false)
		{
			Rect rect = new Rect(mainRect);
			rect.width = 190f;
			rect.yMin += 30f;
			this.DrawControlPane(rect, selectingLandingSite);
			Rect rect2 = new Rect(mainRect);
			rect2.xMin += 204f;
			rect2.yMin += 30f;
			this.DrawWorldView(rect2);
			Rect rect3 = new Rect(rect2);
			rect3.height = 30f;
			rect3.y -= 30f;
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Small;
			Widgets.Label(rect3, Find.World.info.name);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect4 = new Rect(rect2);
			rect4.height = 30f;
			rect4.y -= 30f;
			Text.Anchor = TextAnchor.MiddleRight;
			Text.Font = GameFont.Tiny;
			GUI.color = Color.gray;
			Widgets.Label(rect4, Find.World.info.seedString);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawControlPane(Rect rect, bool selectingLandingSite)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.BeginGroup(rect);
			if (this.selectedCoords.IsValid)
			{
				Rect rect2 = new Rect(0f, 0f, rect.width, 400f);
				this.DrawSquareInfo(rect2, this.selectedCoords);
			}
			Rect rect3 = new Rect(0f, 400f, 80f, 32f);
			Widgets.Label(rect3, "WorldRenderMode".Translate());
			Rect rect4 = new Rect(rect3.xMax, rect3.y, rect.width - rect3.xMax, rect3.height);
			if (Widgets.ButtonText(rect4, Find.World.renderer.CurMode.Label, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (WorldRenderMode current in WorldRenderModeDatabase.AllModes)
				{
					WorldRenderMode localMode = current;
					FloatMenuOption item = new FloatMenuOption(localMode.Label, delegate
					{
						Find.World.renderer.CurMode = localMode;
					}, MenuOptionPriority.Medium, null, null, 0f, null);
					list.Add(item);
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (selectingLandingSite)
			{
				Rect rect5 = new Rect(0f, rect4.yMax + 15f, rect.width, 32f);
				if (Widgets.ButtonText(rect5, "SelectRandomSite".Translate(), true, false, true))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					this.selectedCoords = WorldSquareFinder.RandomStartingWorldSquare();
				}
			}
			GUI.EndGroup();
		}

		private void DrawSquareInfo(Rect rect, IntVec2 wc)
		{
			if (this.selectedCoords.IsValid)
			{
				Rect butRect = new Rect(rect.x, rect.y, 24f, 24f);
				if (Widgets.ButtonImage(butRect, TexButton.Info))
				{
					Find.WindowStack.Add(new Dialog_InfoCard(Find.World.grid.Get(this.selectedCoords).biome));
				}
				rect.yMin += 28f;
			}
			StringBuilder stringBuilder = new StringBuilder();
			WorldSquare worldSquare = Find.World.grid.Get(wc);
			stringBuilder.AppendLine(worldSquare.biome.LabelCap);
			if (worldSquare.biome.canBuildBase)
			{
				stringBuilder.AppendLine("Terrain".Translate() + ": " + worldSquare.hilliness.GetLabel());
			}
			stringBuilder.AppendLine();
			if (Prefs.DevMode)
			{
				stringBuilder.AppendLine("Debug coords: " + wc);
			}
			Vector2 vector = Find.World.LongLatOf(wc);
			stringBuilder.AppendLine(vector.x.ToString("F2") + "°E " + vector.y.ToString("F2") + "°N");
			stringBuilder.AppendLine("Elevation".Translate() + ": " + worldSquare.elevation.ToString("F0") + "m");
			stringBuilder.AppendLine("AvgTemp".Translate() + ": " + worldSquare.temperature.ToStringTemperature("F1"));
			float celsiusTemp = GenTemperature.AverageTemperatureAtWorldCoordsForMonth(wc, Month.Jan);
			stringBuilder.AppendLine("AvgWinterTemp".Translate(new object[]
			{
				celsiusTemp.ToStringTemperature("F1")
			}));
			float celsiusTemp2 = GenTemperature.AverageTemperatureAtWorldCoordsForMonth(wc, Month.Jul);
			stringBuilder.AppendLine("AvgSummerTemp".Translate(new object[]
			{
				celsiusTemp2.ToStringTemperature("F1")
			}));
			if (worldSquare.biome.canBuildBase)
			{
				stringBuilder.AppendLine("Rainfall".Translate() + ": " + worldSquare.rainfall.ToString("F0") + "mm");
			}
			if (!worldSquare.biome.implemented)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(worldSquare.biome.LabelCap + " " + "BiomeNotImplemented".Translate());
			}
			Rot4 rot = Find.World.CoastDirectionAt(wc);
			if (rot.IsValid)
			{
				stringBuilder.AppendLine(("HasCoast" + rot.ToString()).Translate());
			}
			if (worldSquare.biome.canBuildBase)
			{
				stringBuilder.AppendLine("StoneTypesHere".Translate() + ": " + GenText.ToCommaList(from r in Find.World.NaturalRockTypesIn(wc)
				select r.label, true));
			}
			stringBuilder.AppendLine(Zone_Growing.GrowingMonthsDescription(wc));
			Faction faction = Find.World.factionManager.FactionInWorldSquare(wc);
			if (faction != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					"FactionBase".Translate(),
					": ",
					faction.Name,
					" (",
					faction.def.label,
					")"
				}));
			}
			Widgets.Label(rect, stringBuilder.ToString());
		}

		private void DrawWorldView(Rect worldRect)
		{
			Widgets.DrawWindowBackground(worldRect);
			worldRect = worldRect.ContractedBy(1f);
			GUI.BeginGroup(worldRect);
			Rect rect = new Rect(0f, 0f, worldRect.width, worldRect.height);
			if (this.view == null)
			{
				this.view = new WorldView(rect, Find.World.Size.ToVector2() * 0.5f, (float)Find.World.Size.x);
			}
			Find.World.renderer.Draw(this.view);
			if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
			{
				Event.current.Use();
				this.TrySelectWorldSquare(this.view.WorldSquareAt(Event.current.mousePosition));
			}
			KnowledgeAmount knowledgeAmount = KnowledgeAmount.None;
			if (Event.current.type == EventType.KeyDown)
			{
				if (KeyBindingDefOf.MapDollyDown.IsDown)
				{
					this.view.TryDolly(new Vector2(0f, -1f) * 0.1f * this.view.worldRectWidth);
					knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				}
				if (KeyBindingDefOf.MapDollyUp.IsDown)
				{
					this.view.TryDolly(new Vector2(0f, 1f) * 0.1f * this.view.worldRectWidth);
					knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				}
				if (KeyBindingDefOf.MapDollyLeft.IsDown)
				{
					this.view.TryDolly(new Vector2(1f, 0f) * 0.1f * this.view.worldRectWidth);
					knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				}
				if (KeyBindingDefOf.MapDollyRight.IsDown)
				{
					this.view.TryDolly(new Vector2(-1f, 0f) * 0.1f * this.view.worldRectWidth);
					knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				}
				if (KeyBindingDefOf.MapZoomIn.KeyDownEvent)
				{
					this.view.TryZoom(-20f);
					knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				}
				if (KeyBindingDefOf.MapZoomOut.KeyDownEvent)
				{
					this.view.TryZoom(20f);
					knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				}
			}
			if (Event.current.button == 2)
			{
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
				{
					this.viewDragging = true;
					this.viewDragStartLoc = Event.current.mousePosition;
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseUp)
				{
					this.viewDragging = false;
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseDrag)
				{
					if (this.viewDragging && (this.viewDragStartLoc - Event.current.mousePosition).magnitude > 5f)
					{
						Vector2 vector = Event.current.delta;
						vector *= 0.0015f;
						vector *= this.view.worldRectWidth;
						this.view.TryDolly(vector);
						knowledgeAmount = KnowledgeAmount.FrameDisplayed;
					}
					Event.current.Use();
				}
			}
			if (Event.current.type == EventType.ScrollWheel)
			{
				this.view.TryZoom(Event.current.delta.y * 3.5f);
				knowledgeAmount = KnowledgeAmount.SpecificInteraction;
				Event.current.Use();
			}
			if (knowledgeAmount > KnowledgeAmount.None)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, knowledgeAmount);
			}
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
			{
				this.viewDragging = false;
				this.selectedCoords = IntVec2.Invalid;
				Event.current.Use();
			}
			if (this.selectedCoords.IsValid)
			{
				Vector2 vector2 = this.view.ScreenLocOf(this.selectedCoords);
				float pixelsPerWorldSquare = this.view.PixelsPerWorldSquare;
				Rect position = new Rect(vector2.x - pixelsPerWorldSquare * 0.75f, vector2.y - pixelsPerWorldSquare * 0.75f, pixelsPerWorldSquare * 1.5f, pixelsPerWorldSquare * 1.5f);
				GUI.DrawTexture(position, WorldInterface.SelectedSquareOverlay);
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Vector2 vector3 = this.view.ScreenLocOf(Find.Map.WorldCoords);
				float pixelsPerWorldSquare2 = this.view.PixelsPerWorldSquare;
				Rect position2 = new Rect(vector3.x - pixelsPerWorldSquare2 * 0.75f, vector3.y - pixelsPerWorldSquare2 * 0.75f, pixelsPerWorldSquare2 * 1.5f, pixelsPerWorldSquare2 * 1.5f);
				GUI.DrawTexture(position2, WorldInterface.CurMapSquareOverlay);
			}
			GUI.EndGroup();
		}

		private bool TrySelectWorldSquare(IntVec2 c)
		{
			if (!Find.World.InBounds(c))
			{
				return false;
			}
			this.selectedCoords = c;
			SoundDefOf.ThingSelected.PlayOneShotOnCamera();
			return true;
		}
	}
}
