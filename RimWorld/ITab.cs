using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class ITab : InspectTabBase
	{
		protected object SelObject
		{
			get
			{
				return Find.Selector.SingleSelectedObject;
			}
		}

		protected Thing SelThing
		{
			get
			{
				return Find.Selector.SingleSelectedThing;
			}
		}

		protected Pawn SelPawn
		{
			get
			{
				return this.SelThing as Pawn;
			}
		}

		private MainTabWindow_Inspect InspectPane
		{
			get
			{
				return (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
			}
		}

		protected override bool StillValid
		{
			get
			{
				return Find.MainTabsRoot.OpenTab == MainButtonDefOf.Inspect && ((MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.TabWindow).CurTabs.Contains(this);
			}
		}

		protected override float PaneTopY
		{
			get
			{
				return this.InspectPane.PaneTopY;
			}
		}

		protected override void CloseTab()
		{
			this.InspectPane.CloseOpenTab();
		}
	}
}
