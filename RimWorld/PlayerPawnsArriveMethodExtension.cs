using System;
using Verse;

namespace RimWorld
{
	public static class PlayerPawnsArriveMethodExtension
	{
		public static string ToStringHuman(this PlayerPawnsArriveMethod method)
		{
			if (method == PlayerPawnsArriveMethod.Standing)
			{
				return "PlayerPawnsArriveMethod_Standing".Translate();
			}
			if (method != PlayerPawnsArriveMethod.DropPods)
			{
				throw new NotImplementedException();
			}
			return "PlayerPawnsArriveMethod_DropPods".Translate();
		}
	}
}
