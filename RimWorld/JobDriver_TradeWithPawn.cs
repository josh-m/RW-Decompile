using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TradeWithPawn : JobDriver
	{
		private Pawn Trader
		{
			get
			{
				return (Pawn)base.TargetThingA;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Trader, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.$this.Trader.CanTradeNow);
			Toil trade = new Toil();
			trade.initAction = delegate
			{
				Pawn actor = trade.actor;
				if (this.$this.Trader.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(actor, this.$this.Trader));
				}
			};
			yield return trade;
		}
	}
}
