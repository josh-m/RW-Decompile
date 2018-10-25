using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestPrisonerRescue : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			int num;
			SitePartDef sitePartDef;
			Faction faction;
			return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null && this.TryFindTile(out num) && SiteMakerHelper.TryFindSiteParams_SingleSitePart(SiteCoreDefOf.PrisonerWillingToJoin, "PrisonerRescueQuestThreat", out sitePartDef, out faction, null, true, null);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.PrisonerWillingToJoin, "PrisonerRescueQuestThreat", tile, null, true, null, true, null);
			if (site == null)
			{
				return false;
			}
			site.sitePartsKnown = true;
			Pawn pawn = PrisonerWillingToJoinQuestUtility.GeneratePrisoner(tile, site.Faction);
			site.GetComponent<PrisonerWillingToJoinComp>().pawn.TryAdd(pawn, true);
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			string text;
			string label;
			this.GetLetterText(pawn, site, site.parts.FirstOrDefault<SitePart>(), randomInRange, out text, out label);
			Find.LetterStack.ReceiveLetter(label, text, this.def.letterDef, site, site.Faction, null);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange prisonerRescueQuestSiteDistanceRange = SiteTuning.PrisonerRescueQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, prisonerRescueQuestSiteDistanceRange.min, prisonerRescueQuestSiteDistanceRange.max, false, false, -1);
		}

		private void GetLetterText(Pawn prisoner, Site site, SitePart sitePart, int days, out string letter, out string label)
		{
			letter = this.def.letterText.Formatted(site.Faction.Name, prisoner.ageTracker.AgeBiologicalYears, prisoner.story.Title, SitePartUtility.GetDescriptionDialogue(site, sitePart), prisoner.Named("PAWN")).AdjustedFor(prisoner, "PAWN").CapitalizeFirst();
			if (PawnUtility.EverBeenColonistOrTameAnimal(prisoner))
			{
				letter = letter + "\n\n" + "PawnWasFormerlyColonist".Translate(prisoner.LabelShort, prisoner);
			}
			string text;
			PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle<Pawn>(prisoner), out text, true, false);
			label = this.def.letterLabel;
			if (!text.NullOrEmpty())
			{
				string text2 = letter;
				letter = string.Concat(new string[]
				{
					text2,
					"\n\n",
					"PawnHasTheseRelationshipsWithColonists".Translate(prisoner.LabelShort, prisoner),
					"\n\n",
					text
				});
				label = label + " " + "RelationshipAppendedLetterSuffix".Translate();
			}
			letter = letter + "\n\n" + "PrisonerRescueTimeout".Translate(days, prisoner.LabelShort, prisoner.Named("PRISONER"));
		}
	}
}
