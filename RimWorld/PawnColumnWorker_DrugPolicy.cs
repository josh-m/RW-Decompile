using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_DrugPolicy : PawnColumnWorker
	{
		private const int TopAreaHeight = 65;

		private const int ManageDrugPoliciesButtonHeight = 32;

		private const string AssigningDrugsTutorHighlightTag = "ButtonAssignDrugs";

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
			if (Widgets.ButtonText(rect2, "ManageDrugPolicies".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(null));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect2, "ManageDrugPolicies");
			UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.drugs == null)
			{
				return;
			}
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float num3 = rect.x;
			Rect rect2 = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);
			string text = pawn.drugs.CurrentPolicy.label;
			if (pawn.story != null && pawn.story.traits != null)
			{
				Trait trait = pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);
				if (trait != null)
				{
					text = text + " (" + trait.Label + ")";
				}
			}
			text = text.Truncate(rect2.width, null);
			if (Widgets.ButtonText(rect2, text, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (DrugPolicy current in Current.Game.drugPolicyDatabase.AllPolicies)
				{
					DrugPolicy localAssignedDrugs = current;
					list.Add(new FloatMenuOption(current.label, delegate
					{
						pawn.drugs.CurrentPolicy = localAssignedDrugs;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}
			num3 += (float)num;
			num3 += 4f;
			Rect rect3 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
			if (Widgets.ButtonText(rect3, "AssignTabEdit".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(pawn.drugs.CurrentPolicy));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
			UIHighlighter.HighlightOpportunity(rect3, "ButtonAssignDrugs");
			num3 += (float)num2;
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(Mathf.CeilToInt(354f), this.GetMinWidth(table), this.GetMaxWidth(table));
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
			return (pawn.drugs != null && pawn.drugs.CurrentPolicy != null) ? pawn.drugs.CurrentPolicy.uniqueId : -2147483648;
		}
	}
}
