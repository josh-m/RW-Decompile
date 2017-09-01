using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_AncientTempleContents : ItemCollectionGenerator
	{
		private const float ArtifactsChance = 0.9f;

		private const float LuciferiumChance = 0.9f;

		private static readonly IntRange ArtifactsCountRange = new IntRange(1, 3);

		private static readonly IntRange LuciferiumCountRange = new IntRange(5, 20);

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			if (Rand.Chance(0.9f))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Luciferium, null);
				thing.stackCount = ItemCollectionGenerator_AncientTempleContents.LuciferiumCountRange.RandomInRange;
				outThings.Add(thing);
			}
			if (Rand.Chance(0.9f))
			{
				int randomInRange = ItemCollectionGenerator_AncientTempleContents.ArtifactsCountRange.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					ThingDef def = ItemCollectionGenerator_Artifacts.artifacts.RandomElement<ThingDef>();
					Thing item = ThingMaker.MakeThing(def, null);
					outThings.Add(item);
				}
			}
		}
	}
}
