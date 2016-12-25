using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainTabsRoot
	{
		private List<MainTabDef> allTabs;

		private int TabButtonsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.allTabs.Count; i++)
				{
					if (this.allTabs[i].showTabButton)
					{
						num++;
					}
				}
				return num;
			}
		}

		public MainTabDef OpenTab
		{
			get
			{
				MainTabWindow mainTabWindow = Find.WindowStack.WindowOfType<MainTabWindow>();
				if (mainTabWindow == null)
				{
					return null;
				}
				return mainTabWindow.def;
			}
		}

		public MainTabsRoot()
		{
			this.allTabs = (from x in DefDatabase<MainTabDef>.AllDefs
			orderby x.order
			select x).ToList<MainTabDef>();
		}

		public void MainTabsOnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			this.DoTabButtons();
			for (int i = 0; i < this.allTabs.Count; i++)
			{
				if ((this.allTabs[i].validWithoutMap || Find.VisibleMap != null) && this.allTabs[i].toggleHotKey != null && this.allTabs[i].toggleHotKey.KeyDownEvent)
				{
					this.InterfaceTryToggleTab(this.allTabs[i], true);
					Event.current.Use();
					break;
				}
			}
		}

		public void HandleLowPriorityShortcuts()
		{
			if (this.OpenTab == MainTabDefOf.Inspect && Find.Selector.NumSelected == 0)
			{
				this.EscapeCurrentTab(false);
			}
			if (Find.VisibleMap == null && this.OpenTab == null)
			{
				this.SetCurrentTab(MainTabDefOf.World, false);
			}
			if (this.OpenTab != MainTabDefOf.World)
			{
				if (Find.Selector.NumSelected == 0 && Event.current.type == EventType.MouseDown && Event.current.button == 1)
				{
					Event.current.Use();
					this.InterfaceTryToggleTab(MainTabDefOf.Architect, true);
				}
				if (this.OpenTab != MainTabDefOf.Inspect && Event.current.type == EventType.MouseDown && Event.current.button != 2)
				{
					this.EscapeCurrentTab(true);
					Find.Selector.ClearSelection();
				}
			}
		}

		public void EscapeCurrentTab(bool playSound = true)
		{
			this.SetCurrentTab(null, playSound);
		}

		public void SetCurrentTab(MainTabDef tab, bool playSound = true)
		{
			if (tab == this.OpenTab)
			{
				return;
			}
			this.ToggleTab(tab, playSound);
		}

		private void InterfaceTryToggleTab(MainTabDef tab, bool playSound = true)
		{
			if (TutorSystem.TutorialMode && tab.canBeTutorDenied && this.OpenTab != tab && !TutorSystem.AllowAction("MainTab-" + tab.defName + "-Open"))
			{
				return;
			}
			this.ToggleTab(tab, playSound);
		}

		public void ToggleTab(MainTabDef newTab, bool playSound = true)
		{
			if (this.OpenTab == null && newTab == null)
			{
				return;
			}
			if (this.OpenTab == newTab)
			{
				Find.WindowStack.TryRemove(this.OpenTab.Window, true);
				if (playSound)
				{
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
			}
			else
			{
				if (this.OpenTab != null)
				{
					Find.WindowStack.TryRemove(this.OpenTab.Window, true);
				}
				if (newTab != null)
				{
					Find.WindowStack.Add(newTab.Window);
				}
				if (playSound)
				{
					if (newTab == null)
					{
						SoundDefOf.TabClose.PlayOneShotOnCamera();
					}
					else
					{
						SoundDefOf.TabOpen.PlayOneShotOnCamera();
					}
				}
				if (TutorSystem.TutorialMode && newTab != null)
				{
					TutorSystem.Notify_Event("Open-MainTab-" + newTab.defName);
				}
			}
			if (this.OpenTab == MainTabDefOf.World)
			{
				Find.Targeter.StopTargeting();
			}
			else
			{
				Find.WorldTargeter.StopTargeting();
			}
		}

		private void DoTabButtons()
		{
			GUI.color = Color.white;
			int tabButtonsCount = this.TabButtonsCount;
			int num = (int)((float)UI.screenWidth / (float)tabButtonsCount);
			int num2 = this.allTabs.FindLastIndex((MainTabDef x) => x.showTabButton);
			int num3 = 0;
			for (int i = 0; i < this.allTabs.Count; i++)
			{
				if (this.allTabs[i].showTabButton)
				{
					int num4 = num;
					if (i == num2)
					{
						num4 = UI.screenWidth - num3;
					}
					this.DoTabButton(this.allTabs[i], (float)num3, (float)num4);
					num3 += num;
				}
			}
		}

		private void DoTabButton(MainTabDef def, float posX, float width)
		{
			Text.Font = GameFont.Small;
			Rect rect = new Rect(posX, (float)(UI.screenHeight - 35), width, 35f);
			string labelCap = def.LabelCap;
			if ((!def.validWithoutMap || def == MainTabDefOf.World) && Find.VisibleMap == null)
			{
				Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
				{
					Event.current.Use();
				}
			}
			else
			{
				SoundDef mouseoverCategory = SoundDefOf.MouseoverCategory;
				if (Widgets.ButtonTextSubtle(rect, labelCap, def.Window.TabButtonBarPercent, -1f, mouseoverCategory))
				{
					this.InterfaceTryToggleTab(def, true);
				}
				if (this.OpenTab != def && !Find.WindowStack.NonImmediateDialogWindowOpen)
				{
					UIHighlighter.HighlightOpportunity(rect, def.cachedHighlightTagClosed);
				}
				if (!def.description.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect, def.description);
				}
			}
		}
	}
}
