using System;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WITab : InspectTabBase
	{
		protected WorldObject SelObject
		{
			get
			{
				return Find.WorldSelector.SingleSelectedObject;
			}
		}

		protected int SelTileID
		{
			get
			{
				return Find.WorldSelector.selectedTile;
			}
		}

		protected Tile SelTile
		{
			get
			{
				return Find.WorldGrid[this.SelTileID];
			}
		}

		protected Caravan SelCaravan
		{
			get
			{
				return this.SelObject as Caravan;
			}
		}

		private WorldInspectPane InspectPane
		{
			get
			{
				return Find.World.UI.inspectPane;
			}
		}

		protected override bool StillValid
		{
			get
			{
				return WorldRendererUtility.WorldRenderedNow && Find.WindowStack.IsOpen<WorldInspectPane>() && this.InspectPane.CurTabs.Contains(this);
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
