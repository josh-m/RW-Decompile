using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GizmoGridDrawer
	{
		public static HashSet<KeyCode> drawnHotKeys = new HashSet<KeyCode>();

		private static float heightDrawn = 0f;

		private static int heightDrawnFrame;

		private static readonly Vector2 GizmoSpacing = new Vector2(5f, 14f);

		private static List<List<Gizmo>> gizmoGroups = new List<List<Gizmo>>();

		private static List<Gizmo> firstGizmos = new List<Gizmo>();

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
			GizmoGridDrawer.gizmoGroups.Clear();
			foreach (Gizmo current in gizmos)
			{
				bool flag = false;
				for (int i = 0; i < GizmoGridDrawer.gizmoGroups.Count; i++)
				{
					if (GizmoGridDrawer.gizmoGroups[i][0].GroupsWith(current))
					{
						flag = true;
						GizmoGridDrawer.gizmoGroups[i].Add(current);
						break;
					}
				}
				if (!flag)
				{
					List<Gizmo> list = new List<Gizmo>();
					list.Add(current);
					GizmoGridDrawer.gizmoGroups.Add(list);
				}
			}
			GizmoGridDrawer.firstGizmos.Clear();
			for (int j = 0; j < GizmoGridDrawer.gizmoGroups.Count; j++)
			{
				List<Gizmo> source = GizmoGridDrawer.gizmoGroups[j];
				Gizmo gizmo = source.FirstOrDefault((Gizmo opt) => !opt.disabled);
				if (gizmo == null)
				{
					gizmo = source.FirstOrDefault<Gizmo>();
				}
				GizmoGridDrawer.firstGizmos.Add(gizmo);
			}
			GizmoGridDrawer.drawnHotKeys.Clear();
			float num = (float)(UI.screenWidth - 140);
			Text.Font = GameFont.Tiny;
			Vector2 topLeft = new Vector2(startX, (float)(UI.screenHeight - 35) - GizmoGridDrawer.GizmoSpacing.y - 75f);
			mouseoverGizmo = null;
			Gizmo interactedGiz = null;
			Event ev = null;
			for (int k = 0; k < GizmoGridDrawer.firstGizmos.Count; k++)
			{
				Gizmo gizmo2 = GizmoGridDrawer.firstGizmos[k];
				if (gizmo2.Visible)
				{
					if (topLeft.x + gizmo2.Width + GizmoGridDrawer.GizmoSpacing.x > num)
					{
						topLeft.x = startX;
						topLeft.y -= 75f + GizmoGridDrawer.GizmoSpacing.x;
					}
					GizmoGridDrawer.heightDrawnFrame = Time.frameCount;
					GizmoGridDrawer.heightDrawn = (float)UI.screenHeight - topLeft.y;
					GizmoResult gizmoResult = gizmo2.GizmoOnGUI(topLeft);
					if (gizmoResult.State == GizmoState.Interacted)
					{
						ev = gizmoResult.InteractEvent;
						interactedGiz = gizmo2;
					}
					if (gizmoResult.State >= GizmoState.Mouseover)
					{
						mouseoverGizmo = gizmo2;
					}
					Rect rect = new Rect(topLeft.x, topLeft.y, gizmo2.Width, 75f + GizmoGridDrawer.GizmoSpacing.y);
					rect = rect.ContractedBy(-12f);
					GenUI.AbsorbClicksInRect(rect);
					topLeft.x += gizmo2.Width + GizmoGridDrawer.GizmoSpacing.x;
				}
			}
			if (interactedGiz != null)
			{
				List<Gizmo> list2 = GizmoGridDrawer.gizmoGroups.First((List<Gizmo> group) => group.Contains(interactedGiz));
				for (int l = 0; l < list2.Count; l++)
				{
					Gizmo gizmo3 = list2[l];
					if (gizmo3 != interactedGiz && !gizmo3.disabled && interactedGiz.InheritInteractionsFrom(gizmo3))
					{
						gizmo3.ProcessInput(ev);
					}
				}
				interactedGiz.ProcessInput(ev);
				Event.current.Use();
			}
		}
	}
}
