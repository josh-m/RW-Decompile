using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestItemStash : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			int num;
			Faction faction;
			return base.CanFireNowSub(parms) && (Find.FactionManager.RandomNonHostileFaction(false, false, false, TechLevel.Undefined) != null && this.TryFindTile(out num)) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.ItemStash, null, out faction, true, null);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Faction faction = parms.faction;
			if (faction == null)
			{
				faction = Find.FactionManager.RandomNonHostileFaction(false, false, false, TechLevel.Undefined);
			}
			if (faction == null)
			{
				return false;
			}
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			SitePartDef sitePart;
			Faction siteFaction;
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(SiteCoreDefOf.ItemStash, (!Rand.Chance(0.15f)) ? "ItemStashQuestThreat" : null, out sitePart, out siteFaction, null, true, null))
			{
				return false;
			}
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			Site site = IncidentWorker_QuestItemStash.CreateSite(tile, sitePart, randomInRange, siteFaction);
			List<Thing> list = this.GenerateItems(siteFaction, site.desiredThreatPoints);
			site.GetComponent<ItemStashContentsComp>().contents.TryAddRangeOrTransfer(list, false, false);
			string letterText = this.GetLetterText(faction, list, randomInRange, site, site.parts.FirstOrDefault<SitePart>());
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, this.def.letterDef, site, faction, null);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange itemStashQuestSiteDistanceRange = SiteTuning.ItemStashQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, itemStashQuestSiteDistanceRange.min, itemStashQuestSiteDistanceRange.max, false, true, -1);
		}

		protected virtual List<Thing> GenerateItems(Faction siteFaction, float siteThreatPoints)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = new FloatRange?(SiteTuning.ItemStashQuestMarketValueRange * SiteTuning.QuestRewardMarketValueThreatPointsFactor.Evaluate(siteThreatPoints));
			return ThingSetMakerDefOf.Reward_ItemStashQuestContents.root.Generate(parms);
		}

		public static Site CreateSite(int tile, SitePartDef sitePart, int days, Faction siteFaction)
		{
			Site site = SiteMaker.MakeSite(SiteCoreDefOf.ItemStash, sitePart, tile, siteFaction, true, null);
			site.sitePartsKnown = true;
			site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
			Find.WorldObjects.Add(site);
			return site;
		}

		private string GetLetterText(Faction alliedFaction, List<Thing> items, int days, Site site, SitePart sitePart)
		{
			string result = string.Format(this.def.letterText, new object[]
			{
				alliedFaction.leader.LabelShort,
				alliedFaction.def.leaderTitle,
				alliedFaction.Name,
				GenLabel.ThingsLabel(items, "  - "),
				days.ToString(),
				SitePartUtility.GetDescriptionDialogue(site, sitePart),
				GenThing.GetMarketValue(items).ToStringMoney(null)
			}).CapitalizeFirst();
			GenThing.TryAppendSingleRewardInfo(ref result, items);
			return result;
		}
	}
}
