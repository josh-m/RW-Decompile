using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ResourcePodCrash : IncidentWorker
	{
		private const int MaxStacks = 8;

		private const float MaxMarketValue = 40f;

		private static ThingDef RandomPodContentsDef()
		{
			Func<ThingDef, bool> isLeather = (ThingDef d) => d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.Leathers);
			Func<ThingDef, bool> isMeat = (ThingDef d) => d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.MeatRaw);
			int numLeathers = DefDatabase<ThingDef>.AllDefs.Where(isLeather).Count<ThingDef>();
			int numMeats = DefDatabase<ThingDef>.AllDefs.Where(isMeat).Count<ThingDef>();
			return (from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && d.tradeability == Tradeability.Stockable && d.equipmentType == EquipmentType.None && d.BaseMarketValue >= 1f && d.BaseMarketValue < 40f && !d.HasComp(typeof(CompHatcher))
			select d).RandomElementByWeight(delegate(ThingDef d)
			{
				float num = 100f;
				if (isLeather(d))
				{
					num *= 5f / (float)numLeathers;
				}
				if (isMeat(d))
				{
					num *= 5f / (float)numMeats;
				}
				return num;
			});
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			ThingDef thingDef = IncidentWorker_ResourcePodCrash.RandomPodContentsDef();
			List<Thing> list = new List<Thing>();
			float num = (float)Rand.Range(150, 900);
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
				list.Add(thing);
				num -= (float)num2 * thingDef.BaseMarketValue;
			}
			while (list.Count < 8 && num > thingDef.BaseMarketValue);
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			DropPodUtility.DropThingsNear(intVec, map, list, 110, false, true, true);
			Find.LetterStack.ReceiveLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterType.Good, new TargetInfo(intVec, map, false), null);
			return true;
		}

		public static void DebugLogPodContentsChoices()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 100; i++)
			{
				stringBuilder.AppendLine(IncidentWorker_ResourcePodCrash.RandomPodContentsDef().LabelCap);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
