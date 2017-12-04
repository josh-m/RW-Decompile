using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class EditWindow_CurveEditor : EditWindow
	{
		private SimpleCurve curve;

		public List<float> debugInputValues;

		private int draggingPointIndex = -1;

		private int draggingButton = -1;

		private const float ViewDragPanSpeed = 0.002f;

		private const float ScrollZoomSpeed = 0.025f;

		private const float PointClickDistanceLimit = 7f;

		private bool DraggingView
		{
			get
			{
				return this.draggingButton >= 0;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600f, 400f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public EditWindow_CurveEditor(SimpleCurve curve, string title)
		{
			this.curve = curve;
			this.optionalTitle = title;
		}

		public override void DoWindowContents(Rect inRect)
		{
			WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
			if (widgetRow.ButtonIcon(TexButton.CenterOnPointsTex, "Center view around points."))
			{
				this.curve.View.SetViewRectAround(this.curve);
			}
			if (widgetRow.ButtonIcon(TexButton.CurveResetTex, "Reset to growth from 0 to 1."))
			{
				List<CurvePoint> list = new List<CurvePoint>();
				list.Add(new CurvePoint(0f, 0f));
				list.Add(new CurvePoint(1f, 1f));
				this.curve.SetPoints(list);
				this.curve.View.SetViewRectAround(this.curve);
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomHor1Tex, "Reset horizontal zoom to 0-1"))
			{
				this.curve.View.rect.xMin = 0f;
				this.curve.View.rect.xMax = 1f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomHor100Tex, "Reset horizontal zoom to 0-100"))
			{
				this.curve.View.rect.xMin = 0f;
				this.curve.View.rect.xMax = 100f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomHor20kTex, "Reset horizontal zoom to 0-20,000"))
			{
				this.curve.View.rect.xMin = 0f;
				this.curve.View.rect.xMax = 20000f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomVer1Tex, "Reset vertical zoom to 0-1"))
			{
				this.curve.View.rect.yMin = 0f;
				this.curve.View.rect.yMax = 1f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomVer100Tex, "Reset vertical zoom to 0-100"))
			{
				this.curve.View.rect.yMin = 0f;
				this.curve.View.rect.yMax = 100f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomVer20kTex, "Reset vertical zoom to 0-20,000"))
			{
				this.curve.View.rect.yMin = 0f;
				this.curve.View.rect.yMax = 20000f;
			}
			Rect screenRect = new Rect(inRect.AtZero());
			screenRect.yMin += 26f;
			screenRect.yMax -= 24f;
			this.DoCurveEditor(screenRect);
		}

		private void DoCurveEditor(Rect screenRect)
		{
			Widgets.DrawMenuSection(screenRect);
			SimpleCurveDrawer.DrawCurve(screenRect, this.curve, null, null, default(Rect));
			Vector2 mousePosition = Event.current.mousePosition;
			if (Mouse.IsOver(screenRect))
			{
				Rect rect = new Rect(mousePosition.x + 8f, mousePosition.y + 18f, 100f, 100f);
				Vector2 v = SimpleCurveDrawer.ScreenToCurveCoords(screenRect, this.curve.View.rect, mousePosition);
				Widgets.Label(rect, v.ToStringTwoDigits());
			}
			Rect rect2 = new Rect(0f, 0f, 50f, 24f);
			rect2.x = screenRect.x;
			rect2.y = screenRect.y + screenRect.height / 2f - 12f;
			string s = Widgets.TextField(rect2, this.curve.View.rect.x.ToString());
			float num;
			if (float.TryParse(s, out num))
			{
				this.curve.View.rect.x = num;
			}
			rect2.x = screenRect.xMax - rect2.width;
			rect2.y = screenRect.y + screenRect.height / 2f - 12f;
			s = Widgets.TextField(rect2, this.curve.View.rect.xMax.ToString());
			if (float.TryParse(s, out num))
			{
				this.curve.View.rect.xMax = num;
			}
			rect2.x = screenRect.x + screenRect.width / 2f - rect2.width / 2f;
			rect2.y = screenRect.yMax - rect2.height;
			s = Widgets.TextField(rect2, this.curve.View.rect.y.ToString());
			if (float.TryParse(s, out num))
			{
				this.curve.View.rect.y = num;
			}
			rect2.x = screenRect.x + screenRect.width / 2f - rect2.width / 2f;
			rect2.y = screenRect.y;
			s = Widgets.TextField(rect2, this.curve.View.rect.yMax.ToString());
			if (float.TryParse(s, out num))
			{
				this.curve.View.rect.yMax = num;
			}
			if (Mouse.IsOver(screenRect))
			{
				if (Event.current.type == EventType.ScrollWheel)
				{
					float num2 = -1f * Event.current.delta.y * 0.025f;
					float num3 = this.curve.View.rect.center.x - this.curve.View.rect.x;
					float num4 = this.curve.View.rect.center.y - this.curve.View.rect.y;
					SimpleCurveView expr_37D_cp_0 = this.curve.View;
					expr_37D_cp_0.rect.xMin = expr_37D_cp_0.rect.xMin + num3 * num2;
					SimpleCurveView expr_39E_cp_0 = this.curve.View;
					expr_39E_cp_0.rect.xMax = expr_39E_cp_0.rect.xMax - num3 * num2;
					SimpleCurveView expr_3BF_cp_0 = this.curve.View;
					expr_3BF_cp_0.rect.yMin = expr_3BF_cp_0.rect.yMin + num4 * num2;
					SimpleCurveView expr_3E0_cp_0 = this.curve.View;
					expr_3E0_cp_0.rect.yMax = expr_3E0_cp_0.rect.yMax - num4 * num2;
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 2))
				{
					List<int> list = this.PointsNearMouse(screenRect).ToList<int>();
					if (list.Any<int>())
					{
						this.draggingPointIndex = list[0];
					}
					else
					{
						this.draggingPointIndex = -1;
					}
					if (this.draggingPointIndex < 0)
					{
						this.draggingButton = Event.current.button;
					}
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
				{
					Vector2 mouseCurveCoords = SimpleCurveDrawer.ScreenToCurveCoords(screenRect, this.curve.View.rect, Event.current.mousePosition);
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					list2.Add(new FloatMenuOption("Add point at " + mouseCurveCoords.ToString(), delegate
					{
						this.curve.Add(new CurvePoint(mouseCurveCoords), true);
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
					foreach (int current in this.PointsNearMouse(screenRect))
					{
						CurvePoint point = this.curve[current];
						list2.Add(new FloatMenuOption("Remove point at " + point.ToString(), delegate
						{
							this.curve.RemovePointNear(point);
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
					Find.WindowStack.Add(new FloatMenu(list2));
					Event.current.Use();
				}
			}
			if (this.draggingPointIndex >= 0)
			{
				this.curve[this.draggingPointIndex] = new CurvePoint(SimpleCurveDrawer.ScreenToCurveCoords(screenRect, this.curve.View.rect, Event.current.mousePosition));
				this.curve.SortPoints();
				if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
				{
					this.draggingPointIndex = -1;
					Event.current.Use();
				}
			}
			if (this.DraggingView)
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					Vector2 delta = Event.current.delta;
					SimpleCurveView expr_691_cp_0 = this.curve.View;
					expr_691_cp_0.rect.x = expr_691_cp_0.rect.x - delta.x * this.curve.View.rect.width * 0.002f;
					SimpleCurveView expr_6D0_cp_0 = this.curve.View;
					expr_6D0_cp_0.rect.y = expr_6D0_cp_0.rect.y + delta.y * this.curve.View.rect.height * 0.002f;
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseUp && Event.current.button == this.draggingButton)
				{
					this.draggingButton = -1;
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable<int> PointsNearMouse(Rect screenRect)
		{
			GUI.BeginGroup(screenRect);
			try
			{
				for (int i = 0; i < this.curve.PointsCount; i++)
				{
					Vector2 screenPoint = SimpleCurveDrawer.CurveToScreenCoordsInsideScreenRect(screenRect, this.curve.View.rect, this.curve[i].Loc);
					if ((screenPoint - Event.current.mousePosition).sqrMagnitude < 49f)
					{
						yield return i;
					}
				}
			}
			finally
			{
				base.<>__Finally0();
			}
		}
	}
}
