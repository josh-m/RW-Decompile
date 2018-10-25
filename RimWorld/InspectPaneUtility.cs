using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class InspectPaneUtility
	{
		private static Dictionary<string, string> truncatedLabelsCached = new Dictionary<string, string>();

		public const float TabWidth = 72f;

		public const float TabHeight = 30f;

		private static readonly Texture2D InspectTabButtonFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.07450981f, 0.08627451f, 0.105882354f, 1f));

		public const float CornerButtonsSize = 24f;

		public const float PaneInnerMargin = 12f;

		public const float PaneHeight = 165f;

		private const int TabMinimum = 6;

		private static List<Thing> selectedThings = new List<Thing>();

		public static void Reset()
		{
			InspectPaneUtility.truncatedLabelsCached.Clear();
		}

		public static float PaneWidthFor(IInspectPane pane)
		{
			if (pane == null)
			{
				return 432f;
			}
			int num = 0;
			foreach (InspectTabBase current in pane.CurTabs)
			{
				if (current.IsVisible)
				{
					num++;
				}
			}
			return 72f * (float)Mathf.Max(6, num);
		}

		public static Vector2 PaneSizeFor(IInspectPane pane)
		{
			return new Vector2(InspectPaneUtility.PaneWidthFor(pane), 165f);
		}

		public static bool CanInspectTogether(object A, object B)
		{
			Thing thing = A as Thing;
			Thing thing2 = B as Thing;
			return thing != null && thing2 != null && thing.def.category != ThingCategory.Pawn && thing.def == thing2.def;
		}

		public static string AdjustedLabelFor(IEnumerable<object> selected, Rect rect)
		{
			Zone zone = selected.First<object>() as Zone;
			string text;
			if (zone != null)
			{
				text = zone.label;
			}
			else
			{
				InspectPaneUtility.selectedThings.Clear();
				foreach (object current in selected)
				{
					Thing thing = current as Thing;
					if (thing != null)
					{
						InspectPaneUtility.selectedThings.Add(thing);
					}
				}
				if (InspectPaneUtility.selectedThings.Count == 1)
				{
					text = InspectPaneUtility.selectedThings[0].LabelCap;
				}
				else
				{
					IEnumerable<IGrouping<string, Thing>> source = from th in InspectPaneUtility.selectedThings
					group th by th.LabelCapNoCount into g
					select g;
					if (source.Count<IGrouping<string, Thing>>() > 1)
					{
						text = "VariousLabel".Translate();
					}
					else
					{
						text = InspectPaneUtility.selectedThings[0].LabelCapNoCount;
					}
					int num = 0;
					for (int i = 0; i < InspectPaneUtility.selectedThings.Count; i++)
					{
						num += InspectPaneUtility.selectedThings[i].stackCount;
					}
					text = text + " x" + num;
				}
				InspectPaneUtility.selectedThings.Clear();
			}
			Text.Font = GameFont.Medium;
			return text.Truncate(rect.width, InspectPaneUtility.truncatedLabelsCached);
		}

		public static void ExtraOnGUI(IInspectPane pane)
		{
			if (pane.AnythingSelected)
			{
				if (KeyBindingDefOf.SelectNextInCell.KeyDownEvent)
				{
					pane.SelectNextInCell();
				}
				if (Current.ProgramState == ProgramState.Playing)
				{
					pane.DrawInspectGizmos();
				}
				InspectPaneUtility.DoTabs(pane);
			}
		}

		public static void UpdateTabs(IInspectPane pane)
		{
			bool flag = false;
			foreach (InspectTabBase current in pane.CurTabs)
			{
				if (current.IsVisible)
				{
					if (current.GetType() == pane.OpenTabType)
					{
						current.TabUpdate();
						flag = true;
					}
				}
			}
			if (!flag)
			{
				pane.CloseOpenTab();
			}
		}

		public static void InspectPaneOnGUI(Rect inRect, IInspectPane pane)
		{
			pane.RecentHeight = 165f;
			if (pane.AnythingSelected)
			{
				try
				{
					Rect rect = inRect.ContractedBy(12f);
					rect.yMin -= 4f;
					rect.yMax += 6f;
					GUI.BeginGroup(rect);
					float num = 0f;
					if (pane.ShouldShowSelectNextInCellButton)
					{
						Rect rect2 = new Rect(rect.width - 24f, 0f, 24f, 24f);
						if (Widgets.ButtonImage(rect2, TexButton.SelectOverlappingNext))
						{
							pane.SelectNextInCell();
						}
						num += 24f;
						TooltipHandler.TipRegion(rect2, "SelectNextInSquareTip".Translate(KeyBindingDefOf.SelectNextInCell.MainKeyLabel));
					}
					pane.DoInspectPaneButtons(rect, ref num);
					Rect rect3 = new Rect(0f, 0f, rect.width - num, 50f);
					string label = pane.GetLabel(rect3);
					rect3.width += 300f;
					Text.Font = GameFont.Medium;
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect3, label);
					if (pane.ShouldShowPaneContents)
					{
						Rect rect4 = rect.AtZero();
						rect4.yMin += 26f;
						pane.DoPaneContents(rect4);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception doing inspect pane: " + ex.ToString(), false);
				}
				finally
				{
					GUI.EndGroup();
				}
			}
		}

		private static void DoTabs(IInspectPane pane)
		{
			try
			{
				float y = pane.PaneTopY - 30f;
				float num = InspectPaneUtility.PaneWidthFor(pane) - 72f;
				float width = 0f;
				bool flag = false;
				foreach (InspectTabBase current in pane.CurTabs)
				{
					if (current.IsVisible)
					{
						Rect rect = new Rect(num, y, 72f, 30f);
						width = num;
						Text.Font = GameFont.Small;
						if (Widgets.ButtonText(rect, current.labelKey.Translate(), true, false, true))
						{
							InspectPaneUtility.InterfaceToggleTab(current, pane);
						}
						bool flag2 = current.GetType() == pane.OpenTabType;
						if (!flag2 && !current.TutorHighlightTagClosed.NullOrEmpty())
						{
							UIHighlighter.HighlightOpportunity(rect, current.TutorHighlightTagClosed);
						}
						if (flag2)
						{
							current.DoTabGUI();
							pane.RecentHeight = 700f;
							flag = true;
						}
						num -= 72f;
					}
				}
				if (flag)
				{
					GUI.DrawTexture(new Rect(0f, y, width, 30f), InspectPaneUtility.InspectTabButtonFillTex);
				}
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(ex.ToString(), 742783, false);
			}
		}

		private static bool IsOpen(InspectTabBase tab, IInspectPane pane)
		{
			return tab.GetType() == pane.OpenTabType;
		}

		private static void ToggleTab(InspectTabBase tab, IInspectPane pane)
		{
			if (InspectPaneUtility.IsOpen(tab, pane) || (tab == null && pane.OpenTabType == null))
			{
				pane.OpenTabType = null;
				SoundDefOf.TabClose.PlayOneShotOnCamera(null);
			}
			else
			{
				tab.OnOpen();
				pane.OpenTabType = tab.GetType();
				SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
			}
		}

		public static InspectTabBase OpenTab(Type inspectTabType)
		{
			MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
			InspectTabBase inspectTabBase = (from t in mainTabWindow_Inspect.CurTabs
			where inspectTabType.IsAssignableFrom(t.GetType())
			select t).FirstOrDefault<InspectTabBase>();
			if (inspectTabBase != null)
			{
				if (Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect)
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Inspect, true);
				}
				if (!InspectPaneUtility.IsOpen(inspectTabBase, mainTabWindow_Inspect))
				{
					InspectPaneUtility.ToggleTab(inspectTabBase, mainTabWindow_Inspect);
				}
			}
			return inspectTabBase;
		}

		private static void InterfaceToggleTab(InspectTabBase tab, IInspectPane pane)
		{
			if (TutorSystem.TutorialMode && !InspectPaneUtility.IsOpen(tab, pane) && !TutorSystem.AllowAction("ITab-" + tab.tutorTag + "-Open"))
			{
				return;
			}
			InspectPaneUtility.ToggleTab(tab, pane);
		}
	}
}
