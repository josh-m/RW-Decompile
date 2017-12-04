using System;

namespace Verse.AI
{
	public class MentalState_Jailbreaker : MentalState
	{
		private const int NoPrisonerToFreeCheckInterval = 500;

		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (this.pawn.IsHashIntervalTick(500) && JailbreakerMentalStateUtility.FindPrisoner(this.pawn) == null)
			{
				base.RecoverFromState();
			}
		}

		public void Notify_InducedPrisonerToEscape()
		{
			base.RecoverFromState();
		}
	}
}
