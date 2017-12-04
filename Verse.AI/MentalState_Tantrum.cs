using RimWorld;
using System;

namespace Verse.AI
{
	public abstract class MentalState_Tantrum : MentalState
	{
		public Thing target;

		protected bool hitTargetAtLeastOnce;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Thing>(ref this.target, "target", false);
			Scribe_Values.Look<bool>(ref this.hitTargetAtLeastOnce, "hitTargetAtLeastOnce", false, false);
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}

		public override void Notify_AttackedTarget(LocalTargetInfo hitTarget)
		{
			base.Notify_AttackedTarget(hitTarget);
			if (this.target != null && hitTarget.Thing == this.target)
			{
				this.hitTargetAtLeastOnce = true;
			}
		}
	}
}
