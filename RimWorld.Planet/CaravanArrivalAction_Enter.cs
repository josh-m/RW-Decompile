using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_Enter : CaravanArrivalAction
	{
		private MapParent mapParent;

		public override string ReportString
		{
			get
			{
				return "CaravanEntering".Translate(new object[]
				{
					this.mapParent.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.mapParent == null || !this.mapParent.Spawned;
			}
		}

		public override bool ArriveOnTouch
		{
			get
			{
				return true;
			}
		}

		public CaravanArrivalAction_Enter()
		{
		}

		public CaravanArrivalAction_Enter(MapParent mapParent)
		{
			this.mapParent = mapParent;
		}

		public override void Arrived(Caravan caravan)
		{
			Map map = this.mapParent.Map;
			if (map == null)
			{
				return;
			}
			Pawn t = caravan.PawnsListForReading[0];
			CaravanDropInventoryMode dropInventoryMode = CaravanDropInventoryMode.DoNotDrop;
			if (this.mapParent.Faction == Faction.OfPlayer)
			{
				dropInventoryMode = CaravanDropInventoryMode.UnloadIndividually;
			}
			CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, dropInventoryMode, false, null);
			if (this.mapParent.def == WorldObjectDefOf.Ambush)
			{
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredAmbushMap".Translate(), "LetterCaravanEnteredAmbushMap".Translate(new object[]
				{
					caravan.Label
				}).CapitalizeFirst(), LetterType.Good, t, null);
			}
			else if (caravan.IsPlayerControlled || this.mapParent.Faction == Faction.OfPlayer)
			{
				Messages.Message("MessageCaravanEnteredWorldObject".Translate(new object[]
				{
					caravan.Label,
					this.mapParent.Label
				}).CapitalizeFirst(), t, MessageSound.Benefit);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.LookReference<MapParent>(ref this.mapParent, "mapParent", false);
		}
	}
}
