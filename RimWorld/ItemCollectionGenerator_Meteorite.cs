using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Meteorite : ItemCollectionGenerator
	{
		public static List<ThingDef> mineables = new List<ThingDef>();

		public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

		private const float PreciousMineableMarketValue = 5f;

		public static void Reset()
		{
			ItemCollectionGenerator_Meteorite.mineables.Clear();
			ItemCollectionGenerator_Meteorite.mineables.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.mineable && x != ThingDefOf.CollapsedRocks
			select x);
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			ItemCollectionGeneratorParams parms2 = parms;
			int? count = parms.count;
			parms2.count = new int?((!count.HasValue) ? ItemCollectionGenerator_Meteorite.MineablesCountRange.RandomInRange : count.Value);
			parms2.extraAllowedDefs = Gen.YieldSingle<ThingDef>(this.FindRandomMineableDef());
			outThings.AddRange(ItemCollectionGeneratorDefOf.Standard.Worker.Generate(parms2));
		}

		private ThingDef FindRandomMineableDef()
		{
			float value = Rand.Value;
			if (value < 0.4f)
			{
				return (from x in ItemCollectionGenerator_Meteorite.mineables
				where !x.building.isResourceRock
				select x).RandomElement<ThingDef>();
			}
			if (value < 0.75f)
			{
				return (from x in ItemCollectionGenerator_Meteorite.mineables
				where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f
				select x).RandomElement<ThingDef>();
			}
			return (from x in ItemCollectionGenerator_Meteorite.mineables
			where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f
			select x).RandomElement<ThingDef>();
		}
	}
}
