using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GizmoGridDrawer
	{
		public static HashSet<KeyCode> drawnHotKeys = new HashSet<KeyCode>();

		private static float heightDrawn;

		private static int heightDrawnFrame;

		private static readonly Vector2 GizmoSpacing = new Vector2(5f, 14f);

		private static List<List<Gizmo>> gizmoGroups = new List<List<Gizmo>>();

		private static List<Gizmo> firstGizmos = new List<Gizmo>();

		private static List<Gizmo> tmpAllGizmos = new List<Gizmo>();

		public static float HeightDrawnRecently
		{
			get
			{
				if (Time.frameCount > GizmoGridDrawer.heightDrawnFrame + 2)
				{
					return 0f;
				}
				return GizmoGridDrawer.heightDrawn;
			}
		}

		public static void DrawGizmoGrid(IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo)
		{
			GizmoGridDrawer.tmpAllGizmos.Clear();
			GizmoGridDrawer.tmpAllGizmos.AddRange(gizmos);
			GizmoGridDrawer.tmpAllGizmos.SortStable((Gizmo lhs, Gizmo rhs) => lhs.order.CompareTo(rhs.order));
			GizmoGridDrawer.gizmoGroups.Clear();
			for (int i = 0; i < GizmoGridDrawer.tmpAllGizmos.Count; i++)
			{
				Gizmo gizmo = GizmoGridDrawer.tmpAllGizmos[i];
				bool flag = false;
				for (int j = 0; j < GizmoGridDrawer.gizmoGroups.Count; j++)
				{
					if (GizmoGridDrawer.gizmoGroups[j][0].GroupsWith(gizmo))
					{
						flag = true;
						GizmoGridDrawer.gizmoGroups[j].Add(gizmo);
						GizmoGridDrawer.gizmoGroups[j][0].MergeWith(gizmo);
						break;
					}
				}
				if (!flag)
				{
					List<Gizmo> list = new List<Gizmo>();
					list.Add(gizmo);
					GizmoGridDrawer.gizmoGroups.Add(list);
				}
			}
			GizmoGridDrawer.firstGizmos.Clear();
			for (int k = 0; k < GizmoGridDrawer.gizmoGroups.Count; k++)
			{
				List<Gizmo> list2 = GizmoGridDrawer.gizmoGroups[k];
				Gizmo gizmo2 = null;
				for (int l = 0; l < list2.Count; l++)
				{
					if (!list2[l].disabled)
					{
						gizmo2 = list2[l];
						break;
					}
				}
				if (gizmo2 == null)
				{
					gizmo2 = list2.FirstOrDefault<Gizmo>();
				}
				if (gizmo2 != null)
				{
					GizmoGridDrawer.firstGizmos.Add(gizmo2);
				}
			}
			GizmoGridDrawer.drawnHotKeys.Clear();
			float num = (float)(UI.screenWidth - 147);
			float maxWidth = num - startX;
			Text.Font = GameFont.Tiny;
			Vector2 topLeft = new Vector2(startX, (float)(UI.screenHeight - 35) - GizmoGridDrawer.GizmoSpacing.y - 75f);
			mouseoverGizmo = null;
			Gizmo interactedGiz = null;
			Event ev = null;
			Gizmo floatMenuGiz = null;
			for (int m = 0; m < GizmoGridDrawer.firstGizmos.Count; m++)
			{
				Gizmo gizmo3 = GizmoGridDrawer.firstGizmos[m];
				if (gizmo3.Visible)
				{
					if (topLeft.x + gizmo3.GetWidth(maxWidth) > num)
					{
						topLeft.x = startX;
						topLeft.y -= 75f + GizmoGridDrawer.GizmoSpacing.x;
					}
					GizmoGridDrawer.heightDrawnFrame = Time.frameCount;
					GizmoGridDrawer.heightDrawn = (float)UI.screenHeight - topLeft.y;
					GizmoResult gizmoResult = gizmo3.GizmoOnGUI(topLeft, maxWidth);
					if (gizmoResult.State == GizmoState.Interacted)
					{
						ev = gizmoResult.InteractEvent;
						interactedGiz = gizmo3;
					}
					else if (gizmoResult.State == GizmoState.OpenedFloatMenu)
					{
						floatMenuGiz = gizmo3;
					}
					if (gizmoResult.State >= GizmoState.Mouseover)
					{
						mouseoverGizmo = gizmo3;
					}
					Rect rect = new Rect(topLeft.x, topLeft.y, gizmo3.GetWidth(maxWidth), 75f + GizmoGridDrawer.GizmoSpacing.y);
					rect = rect.ContractedBy(-12f);
					GenUI.AbsorbClicksInRect(rect);
					topLeft.x += gizmo3.GetWidth(maxWidth) + GizmoGridDrawer.GizmoSpacing.x;
				}
			}
			if (interactedGiz != null)
			{
				List<Gizmo> list3 = GizmoGridDrawer.gizmoGroups.First((List<Gizmo> group) => group.Contains(interactedGiz));
				for (int n = 0; n < list3.Count; n++)
				{
					Gizmo gizmo4 = list3[n];
					if (gizmo4 != interactedGiz && !gizmo4.disabled && interactedGiz.InheritInteractionsFrom(gizmo4))
					{
						gizmo4.ProcessInput(ev);
					}
				}
				interactedGiz.ProcessInput(ev);
				Event.current.Use();
			}
			else if (floatMenuGiz != null)
			{
				List<FloatMenuOption> list4 = new List<FloatMenuOption>();
				foreach (FloatMenuOption current in floatMenuGiz.RightClickFloatMenuOptions)
				{
					list4.Add(current);
				}
				List<Gizmo> list5 = GizmoGridDrawer.gizmoGroups.First((List<Gizmo> group) => group.Contains(floatMenuGiz));
				for (int num2 = 0; num2 < list5.Count; num2++)
				{
					Gizmo gizmo5 = list5[num2];
					if (gizmo5 != floatMenuGiz && !gizmo5.disabled && floatMenuGiz.InheritFloatMenuInteractionsFrom(gizmo5))
					{
						foreach (FloatMenuOption option in gizmo5.RightClickFloatMenuOptions)
						{
							FloatMenuOption floatMenuOption = list4.Find((FloatMenuOption x) => x.Label == option.Label);
							if (floatMenuOption == null)
							{
								list4.Add(option);
							}
							else if (!option.Disabled)
							{
								if (!floatMenuOption.Disabled)
								{
									Action prevAction = floatMenuOption.action;
									Action localOptionAction = option.action;
									floatMenuOption.action = delegate
									{
										prevAction();
										localOptionAction();
									};
								}
								else if (floatMenuOption.Disabled)
								{
									list4[list4.IndexOf(floatMenuOption)] = option;
								}
							}
						}
					}
				}
				Event.current.Use();
				if (list4.Any<FloatMenuOption>())
				{
					Find.WindowStack.Add(new FloatMenu(list4));
				}
			}
			GizmoGridDrawer.gizmoGroups.Clear();
			GizmoGridDrawer.firstGizmos.Clear();
			GizmoGridDrawer.tmpAllGizmos.Clear();
		}
	}
}
