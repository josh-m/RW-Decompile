using System;

namespace Verse.AI
{
	public class MentalState_CorpseObsession : MentalState
	{
		public Corpse corpse;

		private const int AnyCorpseStillValidCheckInterval = 500;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Corpse>(ref this.corpse, "corpse", false);
		}

		public override void MentalStateTick()
		{
			bool flag = false;
			if (this.pawn.IsHashIntervalTick(500) && !CorpseObsessionMentalStateUtility.IsCorpseValid(this.corpse, this.pawn, false))
			{
				this.corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(this.pawn);
				if (this.corpse == null)
				{
					base.RecoverFromState();
					flag = true;
				}
			}
			if (!flag)
			{
				base.MentalStateTick();
			}
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(this.pawn);
		}

		public void Notify_CorpseHauled()
		{
			base.RecoverFromState();
		}
	}
}
