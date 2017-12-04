using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitEscapeShip : CaravanArrivalAction
	{
		private MapParent target;

		public override string ReportString
		{
			get
			{
				return "CaravanVisiting".Translate(new object[]
				{
					this.target.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.target == null || !this.target.Spawned;
			}
		}

		public CaravanArrivalAction_VisitEscapeShip()
		{
		}

		public CaravanArrivalAction_VisitEscapeShip(MapParent target)
		{
			this.target = target;
		}

		public override void Arrived(Caravan caravan)
		{
			if (!this.target.HasMap)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					this.target.SetFaction(Faction.OfPlayer);
					this.DoArrivalAction(caravan);
					Find.LetterStack.ReceiveLetter("EscapeShipFoundLabel".Translate(), "EscapeShipFound".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(this.target.Map.Center, this.target.Map, false), null);
				}, "GeneratingMapForNewEncounter", false, null);
			}
			else
			{
				this.DoArrivalAction(caravan);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<MapParent>(ref this.target, "target", false);
		}

		private void DoArrivalAction(Caravan caravan)
		{
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.target.Tile, null);
			CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.UnloadIndividually, false, null);
			Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
		}
	}
}
