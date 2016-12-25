using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_Manhunter : MentalState
	{
		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AnimalsDontAttackDoors, OpportunityType.Critical);
		}

		public override bool ForceHostileTo(Thing t)
		{
			return t.Faction != null && this.ForceHostileTo(t.Faction);
		}

		public override bool ForceHostileTo(Faction f)
		{
			return f.def.humanlikeFaction;
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
