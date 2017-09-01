using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainTabsRoot
	{
		public MainButtonDef OpenTab
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

		public void HandleLowPriorityShortcuts()
		{
			if (this.OpenTab == MainButtonDefOf.Inspect && (Find.Selector.NumSelected == 0 || WorldRendererUtility.WorldRenderedNow))
			{
				this.EscapeCurrentTab(false);
			}
			if (Find.Selector.NumSelected == 0 && Event.current.type == EventType.MouseDown && Event.current.button == 1 && !WorldRendererUtility.WorldRenderedNow)
			{
				Event.current.Use();
				MainButtonDefOf.Architect.Worker.InterfaceTryActivate();
			}
			if (this.OpenTab != null && this.OpenTab != MainButtonDefOf.Inspect && Event.current.type == EventType.MouseDown && Event.current.button != 2)
			{
				this.EscapeCurrentTab(true);
				if (Event.current.button == 0)
				{
					Find.Selector.ClearSelection();
					Find.WorldSelector.ClearSelection();
				}
			}
		}

		public void EscapeCurrentTab(bool playSound = true)
		{
			this.SetCurrentTab(null, playSound);
		}

		public void SetCurrentTab(MainButtonDef tab, bool playSound = true)
		{
			if (tab == this.OpenTab)
			{
				return;
			}
			this.ToggleTab(tab, playSound);
		}

		public void ToggleTab(MainButtonDef newTab, bool playSound = true)
		{
			if (this.OpenTab == null && newTab == null)
			{
				return;
			}
			if (this.OpenTab == newTab)
			{
				Find.WindowStack.TryRemove(this.OpenTab.TabWindow, true);
				if (playSound)
				{
					SoundDefOf.TabClose.PlayOneShotOnCamera(null);
				}
			}
			else
			{
				if (this.OpenTab != null)
				{
					Find.WindowStack.TryRemove(this.OpenTab.TabWindow, true);
				}
				if (newTab != null)
				{
					Find.WindowStack.Add(newTab.TabWindow);
				}
				if (playSound)
				{
					if (newTab == null)
					{
						SoundDefOf.TabClose.PlayOneShotOnCamera(null);
					}
					else
					{
						SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
					}
				}
				if (TutorSystem.TutorialMode && newTab != null)
				{
					TutorSystem.Notify_Event("Open-MainTab-" + newTab.defName);
				}
			}
		}
	}
}
