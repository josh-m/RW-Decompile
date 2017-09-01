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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.<>f__this.Trader.CanTradeNow);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<trade>__0.actor;
					if (this.<>f__this.Trader.CanTradeNow)
					{
						Find.WindowStack.Add(new Dialog_Trade(actor, this.<>f__this.Trader));
					}
				}
			};
		}
	}
}
