using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct ItemCollectionGeneratorParams
	{
		public TechLevel? techLevel;

		public int? count;

		public IEnumerable<ThingDef> extraAllowedDefs;

		public float? totalMarketValue;

		public PodContentsType? podContentsType;

		public Predicate<ThingDef> validator;

		public TraderKindDef traderDef;

		public int? tile;

		public Faction traderFaction;

		public float? totalNutrition;

		public bool? nonHumanEdibleFoodAllowed;

		public FoodPreferability? minPreferability;

		public object custom;
	}
}
