using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_MurderousRage : MentalState
	{
		public Pawn target;

		private const int NoLongerValidTargetCheckInterval = 120;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.target, "target", false);
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.target = MurderousRageMentalStateUtility.FindPawn(this.pawn);
		}

		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (this.pawn.IsHashIntervalTick(120) && (this.target == null || !this.target.Spawned || !this.pawn.CanReach(this.target, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.ByPawn)))
			{
				base.RecoverFromState();
			}
		}

		public override string GetBeginLetterText()
		{
			if (this.target == null)
			{
				Log.Error("No target. This should have been checked in this mental state's worker.");
				return string.Empty;
			}
			return string.Format(this.def.beginLetter, this.pawn.Label, this.target.LabelShort).AdjustedFor(this.pawn).CapitalizeFirst();
		}
	}
}
