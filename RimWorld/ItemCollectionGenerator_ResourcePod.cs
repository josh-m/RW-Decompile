using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_ResourcePod : ItemCollectionGenerator
	{
		private const int MaxStacks = 7;

		private const float MaxMarketValue = 40f;

		private const float MinMoney = 150f;

		private const float MaxMoney = 600f;

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			ThingDef thingDef = ItemCollectionGenerator_ResourcePod.RandomPodContentsDef();
			float num = Rand.Range(150f, 600f);
			do
			{
				Thing thing = ThingMaker.MakeThing(thingDef, null);
				int num2 = Rand.Range(20, 40);
				if (num2 > thing.def.stackLimit)
				{
					num2 = thing.def.stackLimit;
				}
				if ((float)num2 * thing.def.BaseMarketValue > num)
				{
					num2 = Mathf.FloorToInt(num / thing.def.BaseMarketValue);
				}
				if (num2 == 0)
				{
					num2 = 1;
				}
				thing.stackCount = num2;
				outThings.Add(thing);
				num -= (float)num2 * thingDef.BaseMarketValue;
			}
			while (outThings.Count < 7 && num > thingDef.BaseMarketValue);
		}

		private static IEnumerable<ThingDef> PossiblePodContentsDefs()
		{
			return from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && d.tradeability == Tradeability.Stockable && d.equipmentType == EquipmentType.None && d.BaseMarketValue >= 1f && d.BaseMarketValue < 40f && !d.HasComp(typeof(CompHatcher))
			select d;
		}

		private static ThingDef RandomPodContentsDef()
		{
			int numMeats = (from x in ItemCollectionGenerator_ResourcePod.PossiblePodContentsDefs()
			where x.IsMeat
			select x).Count<ThingDef>();
			int numLeathers = (from x in ItemCollectionGenerator_ResourcePod.PossiblePodContentsDefs()
			where x.IsLeather
			select x).Count<ThingDef>();
			return ItemCollectionGenerator_ResourcePod.PossiblePodContentsDefs().RandomElementByWeight((ThingDef d) => ItemCollectionGeneratorUtility.AdjustedSelectionWeight(d, numMeats, numLeathers));
		}

		public static void DebugLogPossiblePodContentsDefs()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("ThingDefs that can go in the resource pod crash incident.");
			foreach (ThingDef current in ItemCollectionGenerator_ResourcePod.PossiblePodContentsDefs())
			{
				stringBuilder.AppendLine(current.defName);
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DebugLogPodContentsChoices()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 100; i++)
			{
				stringBuilder.AppendLine(ItemCollectionGenerator_ResourcePod.RandomPodContentsDef().LabelCap);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
