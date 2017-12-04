using RimWorld;
using System;

namespace Verse.AI
{
	public abstract class MentalState_InsultingSpree : MentalState
	{
		public Pawn target;

		public bool insultedTargetAtLeastOnce;

		public int lastInsultTicks = -999999;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.target, "target", false);
			Scribe_Values.Look<bool>(ref this.insultedTargetAtLeastOnce, "insultedTargetAtLeastOnce", false, false);
			Scribe_Values.Look<int>(ref this.lastInsultTicks, "lastInsultTicks", 0, false);
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
