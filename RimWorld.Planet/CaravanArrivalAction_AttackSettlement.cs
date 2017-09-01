using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_AttackSettlement : CaravanArrivalAction
	{
		private Settlement settlement;

		public override string ReportString
		{
			get
			{
				return "CaravanAttacking".Translate(new object[]
				{
					this.settlement.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.settlement == null || !this.settlement.Spawned || !this.settlement.Attackable;
			}
		}

		public CaravanArrivalAction_AttackSettlement()
		{
		}

		public CaravanArrivalAction_AttackSettlement(Settlement settlement)
		{
			this.settlement = settlement;
		}

		public override void Arrived(Caravan caravan)
		{
			if (!this.settlement.HasMap)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					this.DoArrivalAction(caravan);
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
			Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
		}

		private void DoArrivalAction(Caravan caravan)
		{
			Pawn t = caravan.PawnsListForReading[0];
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.settlement.Tile, null);
			CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
			Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			if (!this.settlement.Faction.HostileTo(Faction.OfPlayer))
			{
				this.settlement.Faction.SetHostileTo(Faction.OfPlayer, true);
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredEnemyBase".Translate(), "LetterCaravanEnteredEnemyBaseBecameHostile".Translate(new object[]
				{
					caravan.Label,
					this.settlement.Label,
					this.settlement.Faction.Name
				}).CapitalizeFirst(), LetterDefOf.Good, t, null);
			}
			else
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredEnemyBase".Translate(), "LetterCaravanEnteredEnemyBase".Translate(new object[]
				{
					caravan.Label,
					this.settlement.Label
				}).CapitalizeFirst(), LetterDefOf.Good, t, null);
			}
		}
	}
}
