using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class PawnColumnWorker_Text : PawnColumnWorker
	{
		private static NumericStringComparer comparer = new NumericStringComparer();

		protected virtual int Width
		{
			get
			{
				return this.def.width;
			}
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
			string textFor = this.GetTextFor(pawn);
			if (textFor != null)
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Text.WordWrap = false;
				Widgets.Label(rect2, textFor);
				Text.WordWrap = true;
				Text.Anchor = TextAnchor.UpperLeft;
				string tip = this.GetTip(pawn);
				if (!tip.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, tip);
				}
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), this.Width);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return PawnColumnWorker_Text.comparer.Compare(this.GetTextFor(a), this.GetTextFor(a));
		}

		protected abstract string GetTextFor(Pawn pawn);

		protected virtual string GetTip(Pawn pawn)
		{
			return null;
		}
	}
}
