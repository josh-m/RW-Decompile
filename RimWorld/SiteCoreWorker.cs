using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class SiteCoreWorker
	{
		public SiteCoreDef def;

		public static readonly IntVec3 MapSize = new IntVec3(120, 1, 120);

		public virtual void SiteCoreWorkerTick(Site site)
		{
		}

		public virtual void PostMapGenerate(Map map)
		{
		}

		public virtual void VisitAction(Caravan caravan, Site site)
		{
			this.Enter(caravan, site);
		}

		public virtual bool FactionCanOwn(Faction faction)
		{
			return true;
		}

		[DebuggerHidden]
		public IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				string label = (!site.KnownDanger) ? "VisitSite".Translate(new object[]
				{
					site.Label
				}) : "AttackSite".Translate(new object[]
				{
					site.Label
				});
				string label2 = label;
				Action action = delegate
				{
					caravan.pather.StartPath(site.Tile, new CaravanArrivalAction_VisitSite(site), true);
				};
				Site site2 = site;
				yield return new FloatMenuOption(label2, action, MenuOptionPriority.Default, null, null, 0f, null, site2);
				if (Prefs.DevMode)
				{
					label2 = label + " (Dev: instantly)";
					action = delegate
					{
						caravan.Tile = site.Tile;
						caravan.pather.StopDead();
						new CaravanArrivalAction_VisitSite(site).Arrived(caravan);
					};
					site2 = site;
					yield return new FloatMenuOption(label2, action, MenuOptionPriority.Default, null, null, 0f, null, site2);
				}
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
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, SiteCoreWorker.MapSize, null);
			Map map = orGenerateMap;
			CaravanEnterMode enterMode = CaravanEnterMode.Edge;
			bool draftColonists = flag;
			CaravanEnterMapUtility.Enter(caravan, map, enterMode, CaravanDropInventoryMode.DoNotDrop, draftColonists, null);
			if (flag)
			{
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			}
			Messages.Message("MessageCaravanArrivedAtDestination".Translate(new object[]
			{
				caravan.Label
			}).CapitalizeFirst(), t, MessageTypeDefOf.TaskCompletion);
		}
	}
}
