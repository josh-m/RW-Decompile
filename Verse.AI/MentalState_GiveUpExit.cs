using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_GiveUpExit : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
