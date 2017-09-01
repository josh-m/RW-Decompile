using System;
using Verse;

namespace RimWorld
{
	public struct ItemCollectionGeneratorParams
	{
		public TechLevel techLevel;

		public int count;

		public float totalMarketValue;

		public PodContentsType podContentsType;

		public Predicate<ThingDef> validator;

		public TraderKindDef traderDef;

		public int forTile;

		public Faction forFaction;

		public object custom;
	}
}
