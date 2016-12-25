using RimWorld;
using System;

namespace Verse.AI.Group
{
	public class Trigger_NoFightingSappers : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn p = lord.ownedPawns[i];
					if (this.IsFightingSapper(p))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private bool IsFightingSapper(Pawn p)
		{
			return !p.Downed && !p.InMentalState && RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(p.kindDef);
		}
	}
}
