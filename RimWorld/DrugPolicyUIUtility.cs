using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DrugPolicyUIUtility
	{
		public const string AssigningDrugsTutorHighlightTag = "ButtonAssignDrugs";

		public static void DoAssignDrugPolicyButtons(Rect rect, Pawn pawn)
		{
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
			Rect rect3 = rect2;
			Func<Pawn, DrugPolicy> getPayload = (Pawn p) => p.drugs.CurrentPolicy;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>>>(DrugPolicyUIUtility.Button_GenerateMenu);
			string buttonLabel = text.Truncate(rect2.width, null);
			string label = pawn.drugs.CurrentPolicy.label;
			Widgets.Dropdown<Pawn, DrugPolicy>(rect3, pawn, getPayload, menuGenerator, buttonLabel, null, label, null, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}, true);
			num3 += (float)num;
			num3 += 4f;
			Rect rect4 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
			if (Widgets.ButtonText(rect4, "AssignTabEdit".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(pawn.drugs.CurrentPolicy));
			}
			UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
			UIHighlighter.HighlightOpportunity(rect4, "ButtonAssignDrugs");
			num3 += (float)num2;
		}

		[DebuggerHidden]
		private static IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>> Button_GenerateMenu(Pawn pawn)
		{
			foreach (DrugPolicy assignedDrugs in Current.Game.drugPolicyDatabase.AllPolicies)
			{
				yield return new Widgets.DropdownMenuElement<DrugPolicy>
				{
					option = new FloatMenuOption(assignedDrugs.label, delegate
					{
						pawn.drugs.CurrentPolicy = assignedDrugs;
					}, MenuOptionPriority.Default, null, null, 0f, null, null),
					payload = assignedDrugs
				};
			}
		}
	}
}
