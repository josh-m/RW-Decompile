using System;

namespace RimWorld
{
	[DefOf]
	public static class IncidentDefOf
	{
		public static IncidentDef RaidEnemy;

		public static IncidentDef RaidFriendly;

		public static IncidentDef VisitorGroup;

		public static IncidentDef TravelerGroup;

		public static IncidentDef TraderCaravanArrival;

		public static IncidentDef Eclipse;

		public static IncidentDef ToxicFallout;

		public static IncidentDef SolarFlare;

		public static IncidentDef ManhunterPack;

		public static IncidentDef ShipChunkDrop;

		public static IncidentDef OrbitalTraderArrival;

		public static IncidentDef WandererJoin;

		public static IncidentDef Quest_TradeRequest;

		public static IncidentDef Quest_ItemStashAICore;

		public static IncidentDef Quest_JourneyOffer;

		static IncidentDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
		}
	}
}
