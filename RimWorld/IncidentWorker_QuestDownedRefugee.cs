using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestDownedRefugee : IncidentWorker
	{
		private const float NoSitePartChance = 0.3f;

		private const int MinDistance = 2;

		private const int MaxDistance = 15;

		private static readonly string DownedRefugeeQuestThreatTag = "DownedRefugeeQuestThreat";

		private static readonly IntRange TimeoutDaysRange = new IntRange(5, 10);

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			int num;
			Faction faction;
			return base.CanFireNowSub(target) && this.TryFindTile(out num) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.DownedRefugee, null, out faction, true, null);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.DownedRefugee, (!Rand.Chance(0.3f)) ? IncidentWorker_QuestDownedRefugee.DownedRefugeeQuestThreatTag : null, null, true, null);
			if (site == null)
			{
				return false;
			}
			site.Tile = tile;
			Pawn pawn = DownedRefugeeQuestUtility.GenerateRefugee(tile);
			site.GetComponent<DownedRefugeeComp>().pawn.TryAdd(pawn, true);
			int randomInRange = IncidentWorker_QuestDownedRefugee.TimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			string text = string.Format(this.def.letterText.AdjustedFor(pawn), pawn.Label, randomInRange).CapitalizeFirst();
			Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
			if (mostImportantColonyRelative != null)
			{
				PawnRelationDef mostImportantRelation = mostImportantColonyRelative.GetMostImportantRelation(pawn);
				if (mostImportantRelation != null && mostImportantRelation.opinionOffset > 0)
				{
					pawn.relations.relativeInvolvedInRescueQuest = mostImportantColonyRelative;
					text = text + "\n\n" + "RelatedPawnInvolvedInQuest".Translate(new object[]
					{
						mostImportantColonyRelative.LabelShort,
						mostImportantRelation.GetGenderSpecificLabel(pawn)
					}).AdjustedFor(pawn);
				}
				else
				{
					PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
				}
			}
			if (pawn.relations != null)
			{
				pawn.relations.everSeenByPlayer = true;
			}
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, site, null);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			return TileFinder.TryFindNewSiteTile(out tile, 2, 15, true, false, -1);
		}
	}
}
