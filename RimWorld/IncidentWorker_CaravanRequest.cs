using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_CaravanRequest : IncidentWorker
	{
		private const float TravelBufferMultiple = 0.1f;

		private const float TravelBufferAbsolute = 1f;

		private const int MaxTileDistance = 36;

		private static readonly IntRange OfferDurationRange = new IntRange(10, 30);

		private static readonly IntRange BaseValueWantedRange = new IntRange(400, 3000);

		private static readonly SimpleCurve ValueFactorFromWealthCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0.5f),
				true
			},
			{
				new CurvePoint(50000f, 1f),
				true
			},
			{
				new CurvePoint(300000f, 2f),
				true
			}
		};

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			return IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(((Map)target).Tile) != null && base.CanFireNowSub(target);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Settlement settlement = IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(parms.target.Tile);
			if (settlement == null)
			{
				return false;
			}
			CaravanRequestComp component = settlement.GetComponent<CaravanRequestComp>();
			if (!this.GenerateCaravanRequest(component, (Map)parms.target))
			{
				return false;
			}
			Find.LetterStack.ReceiveLetter("LetterLabelCaravanRequest".Translate(), "LetterCaravanRequest".Translate(new object[]
			{
				settlement.Label,
				GenLabel.ThingLabel(component.requestThingDef, null, component.requestCount).CapitalizeFirst(),
				component.rewards[0].LabelCap,
				(component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0")
			}), LetterDefOf.Good, settlement, null);
			return true;
		}

		public bool GenerateCaravanRequest(CaravanRequestComp target, Map map)
		{
			int num = this.RandomOfferDuration(map.Tile, target.parent.Tile);
			if (num < 1)
			{
				return false;
			}
			target.requestThingDef = IncidentWorker_CaravanRequest.RandomRequestedThingDef();
			if (target.requestThingDef == null)
			{
				Log.Error("Attempted to create a caravan request, but couldn't find a valid request object");
				return false;
			}
			target.requestCount = IncidentWorker_CaravanRequest.RandomRequestCount(target.requestThingDef, map);
			target.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
			target.rewards.TryAdd(IncidentWorker_CaravanRequest.GenerateRewardFor(target.requestThingDef, target.requestCount, target.parent.Faction), true);
			target.expiration = Find.TickManager.TicksGame + num;
			return true;
		}

		public static Settlement RandomNearbyTradeableSettlement(int originTile)
		{
			return (from settlement in Find.WorldObjects.Settlements
			where settlement.Visitable && settlement.GetComponent<CaravanRequestComp>() != null && !settlement.GetComponent<CaravanRequestComp>().ActiveRequest && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f && Find.WorldReachability.CanReach(originTile, settlement.Tile)
			select settlement).RandomElementWithFallback(null);
		}

		private static ThingDef RandomRequestedThingDef()
		{
			Func<ThingDef, bool> globalValidator = delegate(ThingDef td)
			{
				if (td.BaseMarketValue / td.BaseMass < 5f)
				{
					return false;
				}
				if (!td.alwaysHaulable)
				{
					return false;
				}
				CompProperties_Rottable compProperties = td.GetCompProperties<CompProperties_Rottable>();
				return (compProperties == null || compProperties.daysToRotStart >= 10f) && td != ThingDefOf.Silver && td.PlayerAcquirable;
			};
			if (Rand.Value < 0.8f)
			{
				ThingDef result = null;
				bool flag = (from td in DefDatabase<ThingDef>.AllDefs
				where (td.IsWithinCategory(ThingCategoryDefOf.FoodMeals) || td.IsWithinCategory(ThingCategoryDefOf.PlantFoodRaw) || td.IsWithinCategory(ThingCategoryDefOf.PlantMatter) || td.IsWithinCategory(ThingCategoryDefOf.ResourcesRaw)) && td.BaseMarketValue < 4f && globalValidator(td)
				select td).TryRandomElement(out result);
				if (flag)
				{
					return result;
				}
			}
			return (from td in DefDatabase<ThingDef>.AllDefs
			where (td.IsWithinCategory(ThingCategoryDefOf.Medicine) || td.IsWithinCategory(ThingCategoryDefOf.Drugs) || td.IsWithinCategory(ThingCategoryDefOf.Weapons) || td.IsWithinCategory(ThingCategoryDefOf.Apparel) || td.IsWithinCategory(ThingCategoryDefOf.ResourcesRaw)) && td.BaseMarketValue >= 4f && globalValidator(td)
			select td).RandomElementWithFallback(null);
		}

		private static int RandomRequestCount(ThingDef thingDef, Map map)
		{
			float num = (float)IncidentWorker_CaravanRequest.BaseValueWantedRange.RandomInRange;
			float wealthTotal = map.wealthWatcher.WealthTotal;
			num *= IncidentWorker_CaravanRequest.ValueFactorFromWealthCurve.Evaluate(wealthTotal);
			return Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue));
		}

		private static Thing GenerateRewardFor(ThingDef thingDef, int quantity, Faction faction)
		{
			TechLevel techLevel = (faction != null) ? faction.def.techLevel : TechLevel.Spacer;
			ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
			parms.count = 1;
			parms.totalMarketValue = thingDef.BaseMarketValue * (float)quantity * Rand.Range(1f, 2f);
			parms.techLevel = techLevel;
			parms.validator = ((ThingDef td) => td != thingDef);
			return ItemCollectionGeneratorDefOf.CaravanRequestRewards.Worker.Generate(parms)[0];
		}

		private int RandomOfferDuration(int tileIdFrom, int tileIdTo)
		{
			int num = IncidentWorker_CaravanRequest.OfferDurationRange.RandomInRange;
			int num2 = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
			float num3 = (float)num2 / 60000f;
			int b = Mathf.CeilToInt(Mathf.Max(num3 + 1f, num3 * 1.1f));
			num = Mathf.Max(num, b);
			if (num > IncidentWorker_CaravanRequest.OfferDurationRange.max)
			{
				return -1;
			}
			return 60000 * num;
		}
	}
}
