using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_Binging : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
