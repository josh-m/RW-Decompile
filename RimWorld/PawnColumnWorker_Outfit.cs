using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Outfit : PawnColumnWorker
	{
		public const int TopAreaHeight = 65;

		public const int ManageOutfitsButtonHeight = 32;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
			if (Widgets.ButtonText(rect2, "ManageOutfits".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageOutfits(null));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Outfits, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect2, "ManageOutfits");
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.outfits == null)
			{
				return;
			}
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float num3 = rect.x;
			bool somethingIsForced = pawn.outfits.forcedHandler.SomethingIsForced;
			Rect rect2 = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);
			if (somethingIsForced)
			{
				rect2.width -= 4f + (float)num2;
			}
			Rect rect3 = rect2;
			Pawn pawn2 = pawn;
			Func<Pawn, Outfit> getPayload = (Pawn p) => p.outfits.CurrentOutfit;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<Outfit>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<Outfit>>>(this.Button_GenerateMenu);
			string buttonLabel = pawn.outfits.CurrentOutfit.label.Truncate(rect2.width, null);
			string label = pawn.outfits.CurrentOutfit.label;
			Widgets.Dropdown<Pawn, Outfit>(rect3, pawn2, getPayload, menuGenerator, buttonLabel, null, label, null, null, true);
			num3 += rect2.width;
			num3 += 4f;
			Rect rect4 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
			if (somethingIsForced)
			{
				if (Widgets.ButtonText(rect4, "ClearForcedApparel".Translate(), true, false, true))
				{
					pawn.outfits.forcedHandler.Reset();
				}
				TooltipHandler.TipRegion(rect4, new TipSignal(delegate
				{
					string text = "ForcedApparel".Translate() + ":\n";
					foreach (Apparel current in pawn.outfits.forcedHandler.ForcedApparel)
					{
						text = text + "\n   " + current.LabelCap;
					}
					return text;
				}, pawn.GetHashCode() * 612));
				num3 += (float)num2;
				num3 += 4f;
			}
			Rect rect5 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
			if (Widgets.ButtonText(rect5, "AssignTabEdit".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageOutfits(pawn.outfits.CurrentOutfit));
			}
			num3 += (float)num2;
		}

		[DebuggerHidden]
		private IEnumerable<Widgets.DropdownMenuElement<Outfit>> Button_GenerateMenu(Pawn pawn)
		{
			foreach (Outfit outfit in Current.Game.outfitDatabase.AllOutfits)
			{
				yield return new Widgets.DropdownMenuElement<Outfit>
				{
					option = new FloatMenuOption(outfit.label, delegate
					{
						pawn.outfits.CurrentOutfit = outfit;
					}, MenuOptionPriority.Default, null, null, 0f, null, null),
					payload = outfit
				};
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(Mathf.CeilToInt(251f), this.GetMinWidth(table), this.GetMaxWidth(table));
		}

		public override int GetMinHeaderHeight(PawnTable table)
		{
			return Mathf.Max(base.GetMinHeaderHeight(table), 65);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			return (pawn.outfits != null && pawn.outfits.CurrentOutfit != null) ? pawn.outfits.CurrentOutfit.uniqueId : -2147483648;
		}
	}
}
