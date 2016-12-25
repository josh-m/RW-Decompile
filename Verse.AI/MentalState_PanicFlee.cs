using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState_PanicFlee : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
