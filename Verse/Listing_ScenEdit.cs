using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Listing_ScenEdit : Listing_Standard
	{
		private Scenario scen;

		public Listing_ScenEdit(Rect rect, Scenario scen) : base(rect)
		{
			this.scen = scen;
		}

		public Rect GetScenPartRect(ScenPart part, float height)
		{
			string label = part.Label;
			Rect rect = base.GetRect(height);
			Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.08f));
			WidgetRow widgetRow = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown, 72f, 0f);
			if (part.def.PlayerAddRemovable && widgetRow.ButtonIcon(TexButton.DeleteX, null))
			{
				this.scen.RemovePart(part);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			if (this.scen.CanReorder(part, ReorderDirection.Up) && widgetRow.ButtonIcon(TexButton.ReorderUp, null))
			{
				this.scen.Reorder(part, ReorderDirection.Up);
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
			}
			if (this.scen.CanReorder(part, ReorderDirection.Down) && widgetRow.ButtonIcon(TexButton.ReorderDown, null))
			{
				this.scen.Reorder(part, ReorderDirection.Down);
				SoundDefOf.TickLow.PlayOneShotOnCamera();
			}
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect2 = rect.LeftPart(0.5f).Rounded();
			rect2.xMax -= 4f;
			Widgets.Label(rect2, label);
			Text.Anchor = TextAnchor.UpperLeft;
			base.Gap(4f);
			return rect.RightPart(0.5f).Rounded();
		}
	}
}
