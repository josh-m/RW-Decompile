using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_WanderOwnRoom : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
