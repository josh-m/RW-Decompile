using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestDownedRefugee : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			int num;
			Faction faction;
			return base.CanFireNowSub(parms) && this.TryFindTile(out num) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.DownedRefugee, null, out faction, true, null);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.DownedRefugee, (!Rand.Chance(0.3f)) ? "DownedRefugeeQuestThreat" : null, tile, null, true, null, true, null);
			if (site == null)
			{
				return false;
			}
			site.sitePartsKnown = true;
			Pawn pawn = DownedRefugeeQuestUtility.GenerateRefugee(tile);
			site.GetComponent<DownedRefugeeComp>().pawn.TryAdd(pawn, true);
			int randomInRange = SiteTuning.QuestSiteRefugeeTimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			string text = this.def.letterLabel;
			string text2 = this.def.letterText.Formatted(randomInRange, pawn.ageTracker.AgeBiologicalYears, pawn.story.Title, SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault<SitePart>()), pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN").CapitalizeFirst();
			Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
			if (mostImportantColonyRelative != null)
			{
				PawnRelationDef mostImportantRelation = mostImportantColonyRelative.GetMostImportantRelation(pawn);
				if (mostImportantRelation != null && mostImportantRelation.opinionOffset > 0)
				{
					pawn.relations.relativeInvolvedInRescueQuest = mostImportantColonyRelative;
					text2 = text2 + "\n\n" + "RelatedPawnInvolvedInQuest".Translate(mostImportantColonyRelative.LabelShort, mostImportantRelation.GetGenderSpecificLabel(pawn), mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
				}
				else
				{
					PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text2, pawn);
				}
				text = text + " " + "RelationshipAppendedLetterSuffix".Translate();
			}
			if (pawn.relations != null)
			{
				pawn.relations.everSeenByPlayer = true;
			}
			Find.LetterStack.ReceiveLetter(text, text2, this.def.letterDef, site, null, null);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange downedRefugeeQuestSiteDistanceRange = SiteTuning.DownedRefugeeQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, downedRefugeeQuestSiteDistanceRange.min, downedRefugeeQuestSiteDistanceRange.max, true, false, -1);
		}
	}
}
