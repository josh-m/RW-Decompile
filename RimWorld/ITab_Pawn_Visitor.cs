using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ITab_Pawn_Visitor : ITab
	{
		private const float CheckboxInterval = 30f;

		private const float CheckboxMargin = 50f;

		public ITab_Pawn_Visitor()
		{
			this.size = new Vector2(280f, 450f);
		}

		protected override void FillTab()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.FrameDisplayed);
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, this.size.x, this.size.y);
			Rect rect2 = rect.ContractedBy(10f);
			rect2.yMin += 24f;
			bool isPrisonerOfColony = base.SelPawn.IsPrisonerOfColony;
			Listing_Standard listing_Standard = new Listing_Standard(rect2);
			Rect rect3 = listing_Standard.GetRect(Text.LineHeight);
			rect3.width *= 0.75f;
			bool getsFood = base.SelPawn.guest.GetsFood;
			Widgets.CheckboxLabeled(rect3, "GetsFood".Translate(), ref getsFood, false);
			base.SelPawn.guest.GetsFood = getsFood;
			listing_Standard.Gap(12f);
			Rect rect4 = listing_Standard.GetRect(28f);
			rect4.width = 140f;
			MedicalCareUtility.MedicalCareSetter(rect4, ref base.SelPawn.playerSettings.medCare);
			listing_Standard.Gap(4f);
			if (isPrisonerOfColony)
			{
				listing_Standard.Label("RecruitmentDifficulty".Translate() + ": " + base.SelPawn.RecruitDifficulty(Faction.OfPlayer, false).ToStringPercent());
				if (base.SelPawn.guilt.IsGuilty)
				{
					listing_Standard.Label("ConsideredGuilty".Translate(new object[]
					{
						base.SelPawn.guilt.TicksUntilInnocent.ToStringTicksToPeriod(true)
					}));
				}
				if (Prefs.DevMode)
				{
					listing_Standard.Label("Dev: Prison break MTB days: " + (int)PrisonBreakUtility.InitiatePrisonBreakMtbDays(base.SelPawn));
				}
				Rect rect5 = listing_Standard.GetRect(200f).Rounded();
				Widgets.DrawMenuSection(rect5, true);
				Rect position = rect5.ContractedBy(10f);
				GUI.BeginGroup(position);
				Rect rect6 = new Rect(0f, 0f, position.width, 30f);
				using (IEnumerator enumerator = Enum.GetValues(typeof(PrisonerInteractionMode)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PrisonerInteractionMode prisonerInteractionMode = (PrisonerInteractionMode)((byte)enumerator.Current);
						if (Widgets.RadioButtonLabeled(rect6, prisonerInteractionMode.GetLabel(), base.SelPawn.guest.interactionMode == prisonerInteractionMode))
						{
							base.SelPawn.guest.interactionMode = prisonerInteractionMode;
						}
						rect6.y += 28f;
					}
				}
				GUI.EndGroup();
			}
			listing_Standard.End();
		}
	}
}
