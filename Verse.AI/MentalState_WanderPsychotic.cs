using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_WanderPsychotic : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
