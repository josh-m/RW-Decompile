using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestBanditCamp : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Faction faction;
			Faction faction2;
			int num;
			return base.CanFireNowSub(parms) && this.TryFindFactions(out faction, out faction2) && this.TryFindTile(out num);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Faction faction;
			Faction faction2;
			if (!this.TryFindFactions(out faction, out faction2))
			{
				return false;
			}
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			Site site = SiteMaker.MakeSite(SiteCoreDefOf.Nothing, SitePartDefOf.Outpost, tile, faction2, true, null);
			site.sitePartsKnown = true;
			List<Thing> list = this.GenerateRewards(faction, site.desiredThreatPoints);
			site.GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(faction, 18, list);
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			string text = string.Format(this.def.letterText, new object[]
			{
				faction.leader.LabelShort,
				faction.def.leaderTitle,
				faction.Name,
				GenLabel.ThingsLabel(list, string.Empty),
				randomInRange.ToString(),
				SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault<SitePart>()),
				GenThing.GetMarketValue(list).ToStringMoney(null)
			}).CapitalizeFirst();
			GenThing.TryAppendSingleRewardInfo(ref text, list);
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, site, faction, null);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange banditCampQuestSiteDistanceRange = SiteTuning.BanditCampQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, banditCampQuestSiteDistanceRange.min, banditCampQuestSiteDistanceRange.max, false, true, -1);
		}

		private List<Thing> GenerateRewards(Faction alliedFaction, float siteThreatPoints)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = new FloatRange?(SiteTuning.BanditCampQuestRewardMarketValueRange * SiteTuning.QuestRewardMarketValueThreatPointsFactor.Evaluate(siteThreatPoints));
			return ThingSetMakerDefOf.Reward_StandardByDropPod.root.Generate(parms);
		}

		private bool TryFindFactions(out Faction alliedFaction, out Faction enemyFaction)
		{
			if ((from x in Find.FactionManager.AllFactions
			where !x.def.hidden && !x.defeated && !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && this.CommonHumanlikeEnemyFactionExists(Faction.OfPlayer, x) && !this.AnyQuestExistsFrom(x)
			select x).TryRandomElement(out alliedFaction))
			{
				enemyFaction = this.CommonHumanlikeEnemyFaction(Faction.OfPlayer, alliedFaction);
				return true;
			}
			alliedFaction = null;
			enemyFaction = null;
			return false;
		}

		private bool AnyQuestExistsFrom(Faction faction)
		{
			List<Site> sites = Find.WorldObjects.Sites;
			for (int i = 0; i < sites.Count; i++)
			{
				DefeatAllEnemiesQuestComp component = sites[i].GetComponent<DefeatAllEnemiesQuestComp>();
				if (component != null && component.Active && component.requestingFaction == faction)
				{
					return true;
				}
			}
			return false;
		}

		private bool CommonHumanlikeEnemyFactionExists(Faction f1, Faction f2)
		{
			return this.CommonHumanlikeEnemyFaction(f1, f2) != null;
		}

		private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2)
		{
			Faction result;
			if ((from x in Find.FactionManager.AllFactions
			where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) && x.HostileTo(f2)
			select x).TryRandomElement(out result))
			{
				return result;
			}
			return null;
		}
	}
}
