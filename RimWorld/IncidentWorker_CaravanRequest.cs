using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_CaravanRequest : IncidentWorker
	{
		private static readonly IntRange OfferDurationRange = new IntRange(10, 30);

		private const float TravelBufferMultiple = 0.1f;

		private const float TravelBufferAbsolute = 1f;

		private const int MaxTileDistance = 36;

		public static readonly IntRange BaseValueWantedRange = new IntRange(400, 3000);

		public static readonly FloatRange RewardMarketValueFactorRange = new FloatRange(1f, 2f);

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

		private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return this.AtLeast2HealthyColonists(map) && IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(map.Tile) != null && base.CanFireNowSub(target);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Settlement settlement = IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(parms.target.Tile);
			if (settlement == null)
			{
				return false;
			}
			CaravanRequestComp component = settlement.GetComponent<CaravanRequestComp>();
			if (!this.TryGenerateCaravanRequest(component, (Map)parms.target))
			{
				return false;
			}
			Find.LetterStack.ReceiveLetter("LetterLabelCaravanRequest".Translate(), "LetterCaravanRequest".Translate(new object[]
			{
				settlement.Label,
				GenLabel.ThingLabel(component.requestThingDef, null, component.requestCount).CapitalizeFirst(),
				component.rewards[0].LabelCap,
				(component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0")
			}), LetterDefOf.PositiveEvent, settlement, null);
			return true;
		}

		public bool TryGenerateCaravanRequest(CaravanRequestComp target, Map map)
		{
			int num = this.RandomOfferDuration(map.Tile, target.parent.Tile);
			if (num < 1)
			{
				return false;
			}
			if (!IncidentWorker_CaravanRequest.TryFindRandomRequestedThingDef(map, out target.requestThingDef, out target.requestCount))
			{
				return false;
			}
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

		private static bool TryFindRandomRequestedThingDef(Map map, out ThingDef thingDef, out int count)
		{
			IncidentWorker_CaravanRequest.requestCountDict.Clear();
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
				if (compProperties != null && compProperties.daysToRotStart < 10f)
				{
					return false;
				}
				if (td.ingestible != null && td.ingestible.HumanEdible)
				{
					return false;
				}
				if (td == ThingDefOf.Silver)
				{
					return false;
				}
				if (!td.PlayerAcquirable)
				{
					return false;
				}
				int num = IncidentWorker_CaravanRequest.RandomRequestCount(td, map);
				IncidentWorker_CaravanRequest.requestCountDict.Add(td, num);
				return PlayerItemAccessibilityUtility.PossiblyAccessible(td, num, map);
			};
			if ((from td in ItemCollectionGeneratorUtility.allGeneratableItems
			where globalValidator(td)
			select td).TryRandomElement(out thingDef))
			{
				count = IncidentWorker_CaravanRequest.requestCountDict[thingDef];
				return true;
			}
			count = 0;
			return false;
		}

		private static int RandomRequestCount(ThingDef thingDef, Map map)
		{
			float num = (float)IncidentWorker_CaravanRequest.BaseValueWantedRange.RandomInRange;
			float wealthTotal = map.wealthWatcher.WealthTotal;
			num *= IncidentWorker_CaravanRequest.ValueFactorFromWealthCurve.Evaluate(wealthTotal);
			return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue)));
		}

		private static Thing GenerateRewardFor(ThingDef thingDef, int quantity, Faction faction)
		{
			TechLevel value = (faction != null) ? faction.def.techLevel : TechLevel.Spacer;
			ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
			parms.totalMarketValue = new float?(thingDef.BaseMarketValue * (float)quantity * IncidentWorker_CaravanRequest.RewardMarketValueFactorRange.RandomInRange);
			parms.techLevel = new TechLevel?(value);
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

		private bool AtLeast2HealthyColonists(Map map)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsFreeColonist)
				{
					if (!HealthAIUtility.ShouldSeekMedicalRest(list[i]))
					{
						num++;
						if (num >= 2)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
