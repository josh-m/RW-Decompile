using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Assign : MainTabWindow_PawnList
	{
		private const float TopAreaHeight = 65f;

		private const int HostilityResponseColumnWidth = 30;

		private const float LongButtonWidth = 250f;

		private const float ShortButtonWidth = 100f;

		private const float SpaceBetweenOutfitsAndDrugPolicy = 10f;

		private const string AssigningDrugsTutorHighlightTag = "ButtonAssignDrugs";

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(1010f, 65f + (float)base.PawnsCount * 30f + 65f);
			}
		}

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			this.DoTopArea(fillRect);
			Rect rect = new Rect(0f, 65f, fillRect.width, fillRect.height - 65f);
			base.DrawRows(rect);
		}

		private void DoTopArea(Rect fillRect)
		{
			Rect position = new Rect(0f, 0f, fillRect.width, 65f);
			GUI.BeginGroup(position);
			Rect rect = new Rect(195f, 0f, 354f, position.height + 3f);
			Rect rect2 = new Rect(rect.xMax + 10f, 0f, 354f, position.height + 3f);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			float height = Mathf.Round(32.5f);
			Rect rect3 = rect;
			rect3.height = height;
			if (Widgets.ButtonText(rect3, "ManageOutfits".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageOutfits(null));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Outfits, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect3, "ManageOutfits");
			Rect rect4 = rect2;
			rect4.height = height;
			if (Widgets.ButtonText(rect4, "ManageDrugPolicies".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(null));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect4, "ManageDrugPolicies");
			UIHighlighter.HighlightOpportunity(rect4, "ButtonAssignDrugs");
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.LowerCenter;
			Widgets.Label(rect, "CurrentOutfit".Translate());
			Widgets.Label(rect2, "CurrentDrugPolicies".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
		}

		protected override void DrawPawnRow(Rect rect, Pawn p)
		{
			Rect rect2 = new Rect(rect.x + 165f, rect.y + 3f, 30f, rect.height);
			HostilityResponseModeUtility.DrawResponseButton(rect2.position, p);
			float num = rect.x + 165f + 30f;
			this.DrawOutfitsConfig(rect, p, ref num);
			num += 10f;
			this.DrawPolicyConfig(rect, p, ref num);
		}

		private void DrawOutfitsConfig(Rect rect, Pawn p, ref float curX)
		{
			bool somethingIsForced = p.outfits.forcedHandler.SomethingIsForced;
			Rect rect2 = new Rect(curX, rect.y + 2f, 250f, rect.height - 4f);
			if (somethingIsForced)
			{
				rect2.width -= 104f;
			}
			if (Widgets.ButtonText(rect2, p.outfits.CurrentOutfit.label, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (Outfit current in Current.Game.outfitDatabase.AllOutfits)
				{
					Outfit localOut = current;
					list.Add(new FloatMenuOption(localOut.label, delegate
					{
						p.outfits.CurrentOutfit = localOut;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			curX += rect2.width;
			curX += 4f;
			Rect rect3 = new Rect(curX, rect.y + 2f, 100f, rect.height - 4f);
			if (somethingIsForced)
			{
				if (Widgets.ButtonText(rect3, "ClearForcedApparel".Translate(), true, false, true))
				{
					p.outfits.forcedHandler.Reset();
				}
				TooltipHandler.TipRegion(rect3, new TipSignal(delegate
				{
					string text = "ForcedApparel".Translate() + ":\n";
					foreach (Apparel current2 in p.outfits.forcedHandler.ForcedApparel)
					{
						text = text + "\n   " + current2.LabelCap;
					}
					return text;
				}, p.GetHashCode() * 612));
				curX += 100f;
				curX += 4f;
			}
			Rect rect4 = new Rect(curX, rect.y + 2f, 100f, rect.height - 4f);
			if (Widgets.ButtonText(rect4, "AssignTabEdit".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageOutfits(p.outfits.CurrentOutfit));
			}
			curX += 100f;
		}

		private void DrawPolicyConfig(Rect rect, Pawn p, ref float curX)
		{
			Rect rect2 = new Rect(curX, rect.y + 2f, 250f, rect.height - 4f);
			string text = p.drugs.CurrentPolicy.label;
			if (p.story != null && p.story.traits != null)
			{
				Trait trait = p.story.traits.GetTrait(TraitDefOf.DrugDesire);
				if (trait != null)
				{
					text = text + " (" + trait.Label + ")";
					if (text.Length > 35)
					{
						text = text.TrimmedToLength(32) + "...";
					}
				}
			}
			if (Widgets.ButtonText(rect2, text, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (DrugPolicy current in Current.Game.drugPolicyDatabase.AllPolicies)
				{
					DrugPolicy localAssignedDrugs = current;
					list.Add(new FloatMenuOption(current.label, delegate
					{
						p.drugs.CurrentPolicy = localAssignedDrugs;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}
			curX += 250f;
			curX += 4f;
			Rect rect3 = new Rect(curX, rect.y + 2f, 100f, rect.height - 4f);
			if (Widgets.ButtonText(rect3, "AssignTabEdit".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(p.drugs.CurrentPolicy));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
			UIHighlighter.HighlightOpportunity(rect3, "ButtonAssignDrugs");
			curX += 100f;
		}
	}
}
