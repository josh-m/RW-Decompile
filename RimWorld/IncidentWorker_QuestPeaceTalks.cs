using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestPeaceTalks : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Faction faction;
			int num;
			return base.CanFireNowSub(parms) && this.TryFindFaction(out faction) && this.TryFindTile(out num);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Faction faction;
			if (!this.TryFindFaction(out faction))
			{
				return false;
			}
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			PeaceTalks peaceTalks = (PeaceTalks)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PeaceTalks);
			peaceTalks.Tile = tile;
			peaceTalks.SetFaction(faction);
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			peaceTalks.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(peaceTalks);
			string text = this.def.letterText.Formatted(faction.def.leaderTitle, faction.Name, randomInRange, faction.leader.Named("PAWN")).AdjustedFor(faction.leader, "PAWN").CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, peaceTalks, faction, null);
			return true;
		}

		private bool TryFindFaction(out Faction faction)
		{
			return (from x in Find.FactionManager.AllFactions
			where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && x.HostileTo(Faction.OfPlayer) && !x.defeated && !SettlementUtility.IsPlayerAttackingAnySettlementOf(x) && !this.PeaceTalksExist(x) && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned
			select x).TryRandomElement(out faction);
		}

		private bool TryFindTile(out int tile)
		{
			IntRange peaceTalksQuestSiteDistanceRange = SiteTuning.PeaceTalksQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, peaceTalksQuestSiteDistanceRange.min, peaceTalksQuestSiteDistanceRange.max, false, false, -1);
		}

		private bool PeaceTalksExist(Faction faction)
		{
			List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
			for (int i = 0; i < peaceTalks.Count; i++)
			{
				if (peaceTalks[i].Faction == faction)
				{
					return true;
				}
			}
			return false;
		}
	}
}
