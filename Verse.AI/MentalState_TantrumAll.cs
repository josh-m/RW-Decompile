using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalState_TantrumAll : MentalState_TantrumRandom
	{
		protected override void GetPotentialTargets(List<Thing> outThings)
		{
			TantrumMentalStateUtility.GetSmashableThingsNear(this.pawn, this.pawn.Position, outThings, this.GetCustomValidator(), 0, 40);
		}
	}
}
