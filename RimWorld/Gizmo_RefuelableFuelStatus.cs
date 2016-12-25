using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	internal class Gizmo_RefuelableFuelStatus : Gizmo
	{
		private const float ArrowScale = 0.5f;

		public CompRefuelable refuelable;

		private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));

		private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

		private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated", true);

		public override float Width
		{
			get
			{
				return 140f;
			}
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft)
		{
			Rect overRect = new Rect(topLeft.x, topLeft.y, this.Width, 75f);
			Find.WindowStack.ImmediateWindow(1523289473, overRect, WindowLayer.GameUI, delegate
			{
				Rect rect = overRect.AtZero().ContractedBy(6f);
				Rect rect2 = rect;
				rect2.height = overRect.height / 2f;
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect2, "FuelLevelGizmoLabel".Translate());
				Rect rect3 = rect;
				rect3.yMin = overRect.height / 2f;
				float fillPercent = this.refuelable.Fuel / this.refuelable.Props.fuelCapacity;
				Widgets.FillableBar(rect3, fillPercent, Gizmo_RefuelableFuelStatus.FullBarTex, Gizmo_RefuelableFuelStatus.EmptyBarTex, false);
				if (this.refuelable.Props.targetFuelLevelConfigurable)
				{
					float num = this.refuelable.TargetFuelLevel / this.refuelable.Props.fuelCapacity;
					float x = rect3.x + num * rect3.width - (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.width * 0.5f / 2f;
					float y = rect3.y - (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.height * 0.5f;
					GUI.DrawTexture(new Rect(x, y, (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.width * 0.5f, (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.height * 0.5f), Gizmo_RefuelableFuelStatus.TargetLevelArrow);
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, this.refuelable.Fuel.ToString("F0") + " / " + this.refuelable.Props.fuelCapacity.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
			}, true, false, 1f);
			return new GizmoResult(GizmoState.Clear);
		}
	}
}
