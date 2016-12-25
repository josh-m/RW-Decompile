using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class TraderCaravanUtility
	{
		public static TraderCaravanRole GetCaravanRole(this Pawn p)
		{
			if (p.kindDef == PawnKindDefOf.Slave)
			{
				return TraderCaravanRole.Chattel;
			}
			if (p.kindDef.trader)
			{
				return TraderCaravanRole.Trader;
			}
			if (p.kindDef.carrier)
			{
				return TraderCaravanRole.Carrier;
			}
			if (p.RaceProps.Animal)
			{
				return TraderCaravanRole.Chattel;
			}
			return TraderCaravanRole.Guard;
		}

		public static Pawn FindTrader(Lord lord)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (lord.ownedPawns[i].GetCaravanRole() == TraderCaravanRole.Trader)
				{
					return lord.ownedPawns[i];
				}
			}
			return null;
		}
	}
}
