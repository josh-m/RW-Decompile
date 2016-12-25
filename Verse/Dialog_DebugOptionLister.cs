using RimWorld;
using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class Dialog_DebugOptionLister : Dialog_OptionLister
	{
		protected void DebugAction(string label, Action action)
		{
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (this.listing.ButtonDebug(label))
			{
				this.Close(true);
				action();
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight += 24f;
			}
		}

		protected void DebugToolMap(string label, Action toolAction)
		{
			if (Find.MainTabsRoot.OpenTab == MainTabDefOf.World)
			{
				return;
			}
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (this.listing.ButtonDebug(label))
			{
				this.Close(true);
				DebugTools.curTool = new DebugTool(label, toolAction, null);
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight += 24f;
			}
		}

		protected void DebugToolMapForPawns(string label, Action<Pawn> pawnAction)
		{
			this.DebugToolMap(label, delegate
			{
				if (UI.MouseCell().InBounds(Find.VisibleMap))
				{
					foreach (Pawn current in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>().ToList<Pawn>())
					{
						pawnAction(current);
					}
				}
			});
		}

		protected void DebugToolWorld(string label, Action toolAction)
		{
			if (Find.MainTabsRoot.OpenTab != MainTabDefOf.World)
			{
				return;
			}
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (this.listing.ButtonDebug(label))
			{
				this.Close(true);
				DebugTools.curTool = new DebugTool(label, toolAction, null);
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight += 24f;
			}
		}

		protected void CheckboxLabeledDebug(string label, ref bool checkOn)
		{
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			this.listing.LabelCheckboxDebug(label, ref checkOn);
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight += 24f;
			}
		}
	}
}
