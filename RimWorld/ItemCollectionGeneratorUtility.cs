using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class ItemCollectionGeneratorUtility
	{
		public static List<ThingDef> allGeneratableItems = new List<ThingDef>();

		public static readonly string SpecialRewardTag = "SpecialReward";

		public static void Reset()
		{
			ItemCollectionGeneratorUtility.allGeneratableItems.Clear();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if ((current.category == ThingCategory.Item || current.Minifiable) && (current.category != ThingCategory.Item || current.EverHaulable) && !current.isUnfinishedThing && !current.IsCorpse && current.PlayerAcquirable && current.graphicData != null && !typeof(MinifiedThing).IsAssignableFrom(current.thingClass))
				{
					ItemCollectionGeneratorUtility.allGeneratableItems.Add(current);
				}
			}
			ItemCollectionGenerator_Weapons.Reset();
			ItemCollectionGenerator_Apparel.Reset();
			ItemCollectionGenerator_RawResources.Reset();
			ItemCollectionGenerator_Artifacts.Reset();
			ItemCollectionGenerator_Food.Reset();
			ItemCollectionGenerator_Meteorite.Reset();
		}

		public static void AssignRandomBaseGenItemQuality(List<Thing> things)
		{
			for (int i = 0; i < things.Count; i++)
			{
				CompQuality compQuality = things[i].TryGetComp<CompQuality>();
				if (compQuality != null)
				{
					compQuality.SetQuality(QualityUtility.RandomBaseGenItemQuality(), ArtGenerationContext.Outsider);
				}
			}
		}

		public static float AdjustedSelectionWeight(ThingDef d, int numMeats, int numLeathers)
		{
			float num = 100f;
			if (d.IsMeat)
			{
				num *= 5f / (float)numMeats;
			}
			if (d.IsLeather)
			{
				num *= 5f / (float)numLeathers;
			}
			return num;
		}
	}
}
