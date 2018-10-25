using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_FoodRestriction : PawnColumnWorker
	{
		private const int TopAreaHeight = 65;

		public const int ManageFoodRestrictionsButtonHeight = 32;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
			if (Widgets.ButtonText(rect2, "ManageFoodRestrictions".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(null));
			}
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.foodRestriction == null)
			{
				return;
			}
			this.DoAssignFoodRestrictionButtons(rect, pawn);
		}

		[DebuggerHidden]
		private IEnumerable<Widgets.DropdownMenuElement<FoodRestriction>> Button_GenerateMenu(Pawn pawn)
		{
			foreach (FoodRestriction foodRestriction in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
			{
				yield return new Widgets.DropdownMenuElement<FoodRestriction>
				{
					option = new FloatMenuOption(foodRestriction.label, delegate
					{
						pawn.foodRestriction.CurrentFoodRestriction = foodRestriction;
					}, MenuOptionPriority.Default, null, null, 0f, null, null),
					payload = foodRestriction
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
			return (pawn.foodRestriction != null && pawn.foodRestriction.CurrentFoodRestriction != null) ? pawn.foodRestriction.CurrentFoodRestriction.id : -2147483648;
		}

		private void DoAssignFoodRestrictionButtons(Rect rect, Pawn pawn)
		{
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float num3 = rect.x;
			Rect rect2 = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);
			Rect rect3 = rect2;
			Func<Pawn, FoodRestriction> getPayload = (Pawn p) => p.foodRestriction.CurrentFoodRestriction;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<FoodRestriction>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<FoodRestriction>>>(this.Button_GenerateMenu);
			string buttonLabel = pawn.foodRestriction.CurrentFoodRestriction.label.Truncate(rect2.width, null);
			string label = pawn.foodRestriction.CurrentFoodRestriction.label;
			Widgets.Dropdown<Pawn, FoodRestriction>(rect3, pawn, getPayload, menuGenerator, buttonLabel, null, label, null, null, true);
			num3 += (float)num;
			num3 += 4f;
			Rect rect4 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
			if (Widgets.ButtonText(rect4, "AssignTabEdit".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(pawn.foodRestriction.CurrentFoodRestriction));
			}
			num3 += (float)num2;
		}
	}
}
