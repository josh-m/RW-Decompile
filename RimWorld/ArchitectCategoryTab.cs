using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ArchitectCategoryTab
	{
		public const float InfoRectHeight = 230f;

		public DesignationCategoryDef def;

		public static Rect InfoRect
		{
			get
			{
				return new Rect(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainTabDefOf.Architect.Window).WinHeight - 230f, 200f, 230f);
			}
		}

		public ArchitectCategoryTab(DesignationCategoryDef def)
		{
			this.def = def;
		}

		public void PanelClosing()
		{
			Find.DesignatorManager.Deselect();
		}

		public void DesignationTabOnGUI()
		{
			if (Find.DesignatorManager.SelectedDesignator != null)
			{
				Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainTabDefOf.Architect.Window).WinHeight - 230f);
			}
			float startX = 210f;
			Gizmo selectedDesignator;
			GizmoGridDrawer.DrawGizmoGrid(this.def.ResolvedAllowedDesignators.Cast<Gizmo>(), startX, out selectedDesignator);
			if (selectedDesignator == null && Find.DesignatorManager.SelectedDesignator != null)
			{
				selectedDesignator = Find.DesignatorManager.SelectedDesignator;
			}
			this.DoInfoBox(ArchitectCategoryTab.InfoRect, (Designator)selectedDesignator);
		}

		protected void DoInfoBox(Rect infoRect, Designator designator)
		{
			Find.WindowStack.ImmediateWindow(32520, infoRect, WindowLayer.GameUI, delegate
			{
				if (designator != null)
				{
					Rect position = infoRect.AtZero().ContractedBy(7f);
					GUI.BeginGroup(position);
					Rect rect = new Rect(0f, 0f, position.width, 999f);
					Text.Font = GameFont.Small;
					Widgets.Label(rect, designator.LabelCap);
					float num = 24f;
					designator.DrawPanelReadout(ref num, position.width);
					Rect rect2 = new Rect(0f, num, position.width, position.height - num);
					string desc = designator.Desc;
					GenText.SetTextSizeToFit(desc, rect2);
					Widgets.Label(rect2, desc);
					GUI.EndGroup();
				}
			}, true, false, 1f);
		}
	}
}
