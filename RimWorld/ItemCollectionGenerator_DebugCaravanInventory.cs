using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_DebugCaravanInventory : ItemCollectionGenerator
	{
		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int num = Rand.Range(-300, 3000);
			if (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver, null);
				thing.stackCount = num;
				outThings.Add(thing);
			}
		}
	}
}
