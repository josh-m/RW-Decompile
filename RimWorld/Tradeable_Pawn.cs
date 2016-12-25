using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Tradeable_Pawn : Tradeable
	{
		public override Window NewInfoDialog
		{
			get
			{
				return new Dialog_InfoCard(this.AnyPawn);
			}
		}

		public override string Label
		{
			get
			{
				string text = base.Label;
				if (this.AnyPawn.Name != null && !this.AnyPawn.Name.Numerical)
				{
					text = text + ", " + this.AnyPawn.def.label;
				}
				string text2 = text;
				return string.Concat(new string[]
				{
					text2,
					" (",
					this.AnyPawn.gender.GetLabel(),
					", ",
					this.AnyPawn.ageTracker.AgeBiologicalYearsFloat.ToString("F0"),
					")"
				});
			}
		}

		public override string TipDescription
		{
			get
			{
				string str = this.AnyPawn.MainDesc(true);
				return str + "\n\n" + this.AnyPawn.def.description;
			}
		}

		private Pawn AnyPawn
		{
			get
			{
				return (Pawn)this.AnyThing;
			}
		}

		public override void ResolveTrade()
		{
			if (base.ActionToDo == TradeAction.PlayerSells)
			{
				List<Pawn> list = this.thingsColony.Take(-this.countToDrop).Cast<Pawn>().ToList<Pawn>();
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i];
					pawn.PreTraded(TradeAction.PlayerSells, TradeSession.playerNegotiator, TradeSession.trader);
					TradeSession.trader.AddToStock(pawn, TradeSession.playerNegotiator);
					if (pawn.RaceProps.Humanlike)
					{
						foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
						where x.IsColonist || x.IsPrisonerOfColony
						select x)
						{
							current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowPrisonerSold, null);
						}
					}
				}
			}
			else if (base.ActionToDo == TradeAction.PlayerBuys)
			{
				List<Pawn> list2 = this.thingsTrader.Take(this.countToDrop).Cast<Pawn>().ToList<Pawn>();
				for (int j = 0; j < list2.Count; j++)
				{
					Pawn pawn2 = list2[j];
					TradeSession.trader.GiveSoldThingToPlayer(pawn2, pawn2, TradeSession.playerNegotiator);
					pawn2.PreTraded(TradeAction.PlayerBuys, TradeSession.playerNegotiator, TradeSession.trader);
				}
			}
		}
	}
}
