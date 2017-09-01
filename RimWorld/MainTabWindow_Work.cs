using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Work : MainTabWindow_PawnTable
	{
		private const int SpaceBetweenPriorityArrowsAndWorkLabels = 40;

		protected override PawnTableDef PawnTableDef
		{
			get
			{
				return PawnTableDefOf.Work;
			}
		}

		protected override float ExtraTopSpace
		{
			get
			{
				return 40f;
			}
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DoWindowContents(rect);
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			this.DoManualPrioritiesCheckbox();
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Tiny;
			Rect rect2 = new Rect(370f, rect.y + 5f, 160f, 30f);
			Widgets.Label(rect2, "<= " + "HigherPriority".Translate());
			Rect rect3 = new Rect(630f, rect.y + 5f, 160f, 30f);
			Widgets.Label(rect3, "LowerPriority".Translate() + " =>");
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DoManualPrioritiesCheckbox()
		{
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect = new Rect(5f, 5f, 140f, 30f);
			bool useWorkPriorities = Current.Game.playSettings.useWorkPriorities;
			Widgets.CheckboxLabeled(rect, "ManualPriorities".Translate(), ref Current.Game.playSettings.useWorkPriorities, false);
			if (useWorkPriorities != Current.Game.playSettings.useWorkPriorities)
			{
				foreach (Pawn current in PawnsFinder.AllMapsAndWorld_Alive)
				{
					if (current.Faction == Faction.OfPlayer && current.workSettings != null)
					{
						current.workSettings.Notify_UseWorkPrioritiesChanged();
					}
				}
			}
			if (!Current.Game.playSettings.useWorkPriorities)
			{
				UIHighlighter.HighlightOpportunity(rect, "ManualPriorities-Off");
			}
		}
	}
}
