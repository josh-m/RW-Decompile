using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class SiteCoreWorker : SiteCoreOrPartWorkerBase
	{
		public static readonly IntVec3 MapSize = new IntVec3(120, 1, 120);

		private static List<SiteCoreOrPartDefBase> tmpDefs = new List<SiteCoreOrPartDefBase>();

		private static List<SiteCoreOrPartDefBase> tmpUsedDefs = new List<SiteCoreOrPartDefBase>();

		public SiteCoreDef Def
		{
			get
			{
				return (SiteCoreDef)this.def;
			}
		}

		public virtual void SiteCoreWorkerTick(Site site)
		{
		}

		public virtual void VisitAction(Caravan caravan, Site site)
		{
			this.Enter(caravan, site);
		}

		[DebuggerHidden]
		public IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				foreach (FloatMenuOption f in CaravanArrivalAction_VisitSite.GetFloatMenuOptions(caravan, site))
				{
					yield return f;
				}
			}
		}

		[DebuggerHidden]
		public virtual IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative, Site site)
		{
			foreach (FloatMenuOption f in TransportPodsArrivalAction_VisitSite.GetFloatMenuOptions(representative, pods, site))
			{
				yield return f;
			}
		}

		protected void Enter(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					this.DoEnter(caravan, site);
				}, "GeneratingMapForNewEncounter", false, null);
			}
			else
			{
				this.DoEnter(caravan, site);
			}
		}

		private void DoEnter(Caravan caravan, Site site)
		{
			Pawn t = caravan.PawnsListForReading[0];
			bool flag = site.Faction == null || site.Faction.HostileTo(Faction.OfPlayer);
			bool flag2 = !site.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, SiteCoreWorker.MapSize, null);
			if (flag2)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsSite".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst());
				LetterDef letterDef;
				LookTargets lookTargets;
				this.AppendThreatInfo(stringBuilder, site, orGenerateMap, out letterDef, out lookTargets);
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(site), stringBuilder.ToString(), letterDef ?? LetterDefOf.NeutralEvent, (!lookTargets.IsValid()) ? t : lookTargets, null, null);
			}
			else
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(site), "LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst(), LetterDefOf.NeutralEvent, t, null, null);
			}
			Map map = orGenerateMap;
			CaravanEnterMode enterMode = CaravanEnterMode.Edge;
			bool draftColonists = flag;
			CaravanEnterMapUtility.Enter(caravan, map, enterMode, CaravanDropInventoryMode.DoNotDrop, draftColonists, null);
		}

		private void AppendThreatInfo(StringBuilder sb, Site site, Map map, out LetterDef letterDef, out LookTargets allLookTargets)
		{
			allLookTargets = new LookTargets();
			SiteCoreWorker.tmpUsedDefs.Clear();
			SiteCoreWorker.tmpDefs.Clear();
			SiteCoreWorker.tmpDefs.Add(this.def);
			for (int i = 0; i < site.parts.Count; i++)
			{
				SiteCoreWorker.tmpDefs.Add(site.parts[i].def);
			}
			letterDef = null;
			for (int j = 0; j < SiteCoreWorker.tmpDefs.Count; j++)
			{
				LetterDef letterDef2;
				LookTargets lookTargets;
				string arrivedLetterPart = SiteCoreWorker.tmpDefs[j].Worker.GetArrivedLetterPart(map, out letterDef2, out lookTargets);
				if (arrivedLetterPart != null)
				{
					if (!SiteCoreWorker.tmpUsedDefs.Contains(SiteCoreWorker.tmpDefs[j]))
					{
						SiteCoreWorker.tmpUsedDefs.Add(SiteCoreWorker.tmpDefs[j]);
						if (sb.Length > 0)
						{
							sb.AppendLine();
							sb.AppendLine();
						}
						sb.Append(arrivedLetterPart);
					}
					if (letterDef == null)
					{
						letterDef = letterDef2;
					}
					if (lookTargets.IsValid())
					{
						allLookTargets = new LookTargets(allLookTargets.targets.Concat(lookTargets.targets));
					}
				}
			}
		}
	}
}
