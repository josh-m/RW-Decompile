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
		private const int MaxStacks = 7;

		private const float MaxMarketValue = 40f;

		private const float MinMoney = 150f;

		private const float MaxMoney = 600f;

		private static IEnumerable<ThingDef> PossiblePodContentsDefs()
		{
			return from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && d.tradeability == Tradeability.Stockable && d.equipmentType == EquipmentType.None && d.BaseMarketValue >= 1f && d.BaseMarketValue < 40f && !d.HasComp(typeof(CompHatcher))
			select d;
		}

		private static ThingDef RandomPodContentsDef()
		{
			Func<ThingDef, bool> isLeather = (ThingDef d) => d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.Leathers);
			Func<ThingDef, bool> isMeat = (ThingDef d) => d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.MeatRaw);
			int numLeathers = DefDatabase<ThingDef>.AllDefs.Where(isLeather).Count<ThingDef>();
			int numMeats = DefDatabase<ThingDef>.AllDefs.Where(isMeat).Count<ThingDef>();
			return IncidentWorker_ResourcePodCrash.PossiblePodContentsDefs().RandomElementByWeight(delegate(ThingDef d)
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
				list.Add(thing);
				num -= (float)num2 * thingDef.BaseMarketValue;
			}
			while (list.Count < 7 && num > thingDef.BaseMarketValue);
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			DropPodUtility.DropThingsNear(intVec, map, list, 110, false, true, true);
			Find.LetterStack.ReceiveLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.Good, new TargetInfo(intVec, map, false), null);
			return true;
		}

		public static void DebugLogPossiblePodContentsDefs()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("ThingDefs that can go in the resource pod crash incident.");
			foreach (ThingDef current in IncidentWorker_ResourcePodCrash.PossiblePodContentsDefs())
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
				stringBuilder.AppendLine(IncidentWorker_ResourcePodCrash.RandomPodContentsDef().LabelCap);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
