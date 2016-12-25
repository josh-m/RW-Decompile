using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Designator_Place : Designator
	{
		private const float RotButSize = 64f;

		private const float RotButSpacing = 10f;

		protected Rot4 placingRot = Rot4.North;

		protected static float middleMouseDownTime;

		public abstract BuildableDef PlacingDef
		{
			get;
		}

		public Designator_Place()
		{
			this.soundDragSustain = SoundDefOf.DesignateDragBuilding;
			this.soundDragChanged = SoundDefOf.DesignateDragBuildingChanged;
			this.soundSucceeded = SoundDefOf.DesignatePlaceBuilding;
		}

		public override void DoExtraGuiControls(float leftX, float bottomY)
		{
			ThingDef thingDef = this.PlacingDef as ThingDef;
			if (thingDef != null && thingDef.rotatable)
			{
				Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
				this.HandleRotationShortcuts();
				Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
				{
					RotationDirection rotationDirection = RotationDirection.None;
					Text.Anchor = TextAnchor.MiddleCenter;
					Text.Font = GameFont.Medium;
					Rect rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
					if (Widgets.ButtonImage(rect, TexUI.RotLeftTex))
					{
						SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
						rotationDirection = RotationDirection.Counterclockwise;
						Event.current.Use();
					}
					Widgets.Label(rect, KeyBindingDefOf.DesignatorRotateLeft.MainKeyLabel);
					Rect rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
					if (Widgets.ButtonImage(rect2, TexUI.RotRightTex))
					{
						SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
						rotationDirection = RotationDirection.Clockwise;
						Event.current.Use();
					}
					Widgets.Label(rect2, KeyBindingDefOf.DesignatorRotateRight.MainKeyLabel);
					if (rotationDirection != RotationDirection.None)
					{
						this.placingRot.Rotate(rotationDirection);
					}
					Text.Anchor = TextAnchor.UpperLeft;
					Text.Font = GameFont.Small;
				}, true, false, 1f);
			}
		}

		public override void SelectedUpdate()
		{
			GenDraw.DrawNoBuildEdgeLines();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				IntVec3 intVec = UI.MouseCell();
				if (this.PlacingDef is TerrainDef)
				{
					GenUI.RenderMouseoverBracket();
					return;
				}
				Color ghostCol;
				if (this.CanDesignateCell(intVec).Accepted)
				{
					ghostCol = new Color(0.5f, 1f, 0.6f, 0.4f);
				}
				else
				{
					ghostCol = new Color(1f, 0f, 0f, 0.4f);
				}
				this.DrawGhost(ghostCol);
				if (this.CanDesignateCell(intVec).Accepted && this.PlacingDef.specialDisplayRadius > 0.01f)
				{
					GenDraw.DrawRadiusRing(UI.MouseCell(), this.PlacingDef.specialDisplayRadius);
				}
				GenDraw.DrawInteractionCell((ThingDef)this.PlacingDef, intVec, this.placingRot);
			}
		}

		protected virtual void DrawGhost(Color ghostCol)
		{
			GhostDrawer.DrawGhostThing(UI.MouseCell(), this.placingRot, (ThingDef)this.PlacingDef, null, ghostCol, AltitudeLayer.Blueprint);
		}

		private void HandleRotationShortcuts()
		{
			RotationDirection rotationDirection = RotationDirection.None;
			if (Event.current.button == 2)
			{
				if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
					Designator_Place.middleMouseDownTime = Time.realtimeSinceStartup;
				}
				if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - Designator_Place.middleMouseDownTime < 0.15f)
				{
					rotationDirection = RotationDirection.Clockwise;
				}
			}
			if (KeyBindingDefOf.DesignatorRotateRight.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Clockwise;
			}
			if (KeyBindingDefOf.DesignatorRotateLeft.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Counterclockwise;
			}
			if (rotationDirection == RotationDirection.Clockwise)
			{
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
				this.placingRot.Rotate(RotationDirection.Clockwise);
			}
			if (rotationDirection == RotationDirection.Counterclockwise)
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
				this.placingRot.Rotate(RotationDirection.Counterclockwise);
			}
		}

		public override void Selected()
		{
			this.placingRot = this.PlacingDef.defaultPlacingRot;
		}
	}
}
