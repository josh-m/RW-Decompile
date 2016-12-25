using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_Attack : CaravanArrivalAction
	{
		private FactionBase factionBase;

		public override string ReportString
		{
			get
			{
				return "CaravanAttacking".Translate(new object[]
				{
					this.factionBase.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.factionBase == null || !this.factionBase.Spawned;
			}
		}

		public override bool ArriveOnTouch
		{
			get
			{
				return true;
			}
		}

		public CaravanArrivalAction_Attack()
		{
		}

		public CaravanArrivalAction_Attack(FactionBase factionBase)
		{
			this.factionBase = factionBase;
		}

		public override void Arrived(Caravan caravan)
		{
			if (this.factionBase.Map == null)
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
			Scribe_References.LookReference<FactionBase>(ref this.factionBase, "factionBase", false);
		}

		private void DoArrivalAction(Caravan caravan)
		{
			Map map = this.factionBase.Map;
			if (map == null)
			{
				map = AttackCaravanArrivalActionUtility.GenerateFactionBaseMap(this.factionBase);
			}
			Pawn t = caravan.PawnsListForReading[0];
			CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
			Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			if (!this.factionBase.Faction.HostileTo(Faction.OfPlayer))
			{
				this.factionBase.Faction.SetHostileTo(Faction.OfPlayer, true);
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredEnemyBase".Translate(), "LetterCaravanEnteredEnemyBaseBecameHostile".Translate(new object[]
				{
					caravan.Label,
					this.factionBase.Label,
					this.factionBase.Faction.Name
				}).CapitalizeFirst(), LetterType.Good, t, null);
			}
			else
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredEnemyBase".Translate(), "LetterCaravanEnteredEnemyBase".Translate(new object[]
				{
					caravan.Label,
					this.factionBase.Label
				}).CapitalizeFirst(), LetterType.Good, t, null);
			}
		}
	}
}
