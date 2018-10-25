using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestTradeRequest : IncidentWorker
	{
		private const float MinNonTravelTimeFractionOfTravelTime = 0.35f;

		private const float MinNonTravelTimeDays = 6f;

		private const int MaxTileDistance = 36;

		private static readonly IntRange BaseValueWantedRange = new IntRange(500, 2500);

		private static readonly SimpleCurve ValueWantedFactorFromWealthCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0.3f),
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

		private static readonly FloatRange RewardValueFactorRange = new FloatRange(1.5f, 2.1f);

		private static readonly SimpleCurve RewardValueFactorFromWealthCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1.15f),
				true
			},
			{
				new CurvePoint(50000f, 1f),
				true
			},
			{
				new CurvePoint(300000f, 0.85f),
				true
			}
		};

		private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();

		private static List<Map> tmpAvailableMaps = new List<Map>();

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map;
			return base.CanFireNowSub(parms) && this.TryGetRandomAvailableTargetMap(out map) && IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(map.Tile) != null;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map;
			if (!this.TryGetRandomAvailableTargetMap(out map))
			{
				return false;
			}
			SettlementBase settlementBase = IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(map.Tile);
			if (settlementBase == null)
			{
				return false;
			}
			TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
			if (!this.TryGenerateTradeRequest(component, map))
			{
				return false;
			}
			string text = "LetterCaravanRequest".Translate(settlementBase.Label, TradeRequestUtility.RequestedThingLabel(component.requestThingDef, component.requestCount).CapitalizeFirst(), (component.requestThingDef.GetStatValueAbstract(StatDefOf.MarketValue, null) * (float)component.requestCount).ToStringMoney("F0"), GenThing.ThingsToCommaList(component.rewards, true, true, -1).CapitalizeFirst(), GenThing.GetMarketValue(component.rewards).ToStringMoney("F0"), (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0"), CaravanArrivalTimeEstimator.EstimatedTicksToArrive(map.Tile, settlementBase.Tile, null).ToStringTicksToDays("0.#"));
			GenThing.TryAppendSingleRewardInfo(ref text, component.rewards);
			Find.LetterStack.ReceiveLetter("LetterLabelCaravanRequest".Translate(), text, LetterDefOf.PositiveEvent, settlementBase, settlementBase.Faction, null);
			return true;
		}

		public bool TryGenerateTradeRequest(TradeRequestComp target, Map map)
		{
			int num = this.RandomOfferDurationTicks(map.Tile, target.parent.Tile);
			if (num < 1)
			{
				return false;
			}
			if (!IncidentWorker_QuestTradeRequest.TryFindRandomRequestedThingDef(map, out target.requestThingDef, out target.requestCount))
			{
				return false;
			}
			target.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
			target.rewards.TryAddRangeOrTransfer(IncidentWorker_QuestTradeRequest.GenerateRewardsFor(target.requestThingDef, target.requestCount, target.parent.Faction, map), true, true);
			target.expiration = Find.TickManager.TicksGame + num;
			return true;
		}

		public static SettlementBase RandomNearbyTradeableSettlement(int originTile)
		{
			return (from settlement in Find.WorldObjects.SettlementBases
			where settlement.Visitable && settlement.GetComponent<TradeRequestComp>() != null && !settlement.GetComponent<TradeRequestComp>().ActiveRequest && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f && Find.WorldReachability.CanReach(originTile, settlement.Tile)
			select settlement).RandomElementWithFallback(null);
		}

		private static bool TryFindRandomRequestedThingDef(Map map, out ThingDef thingDef, out int count)
		{
			IncidentWorker_QuestTradeRequest.requestCountDict.Clear();
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
				int num = IncidentWorker_QuestTradeRequest.RandomRequestCount(td, map);
				IncidentWorker_QuestTradeRequest.requestCountDict.Add(td, num);
				return PlayerItemAccessibilityUtility.PossiblyAccessible(td, num, map) && PlayerItemAccessibilityUtility.PlayerCanMake(td, map) && (td.thingSetMakerTags == null || !td.thingSetMakerTags.Contains("RewardSpecial"));
			};
			if ((from td in ThingSetMakerUtility.allGeneratableItems
			where globalValidator(td)
			select td).TryRandomElement(out thingDef))
			{
				count = IncidentWorker_QuestTradeRequest.requestCountDict[thingDef];
				return true;
			}
			count = 0;
			return false;
		}

		private bool TryGetRandomAvailableTargetMap(out Map map)
		{
			IncidentWorker_QuestTradeRequest.tmpAvailableMaps.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && this.AtLeast2HealthyColonists(maps[i]) && IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(maps[i].Tile) != null)
				{
					IncidentWorker_QuestTradeRequest.tmpAvailableMaps.Add(maps[i]);
				}
			}
			bool result = IncidentWorker_QuestTradeRequest.tmpAvailableMaps.TryRandomElement(out map);
			IncidentWorker_QuestTradeRequest.tmpAvailableMaps.Clear();
			return result;
		}

		private static int RandomRequestCount(ThingDef thingDef, Map map)
		{
			float num = (float)IncidentWorker_QuestTradeRequest.BaseValueWantedRange.RandomInRange;
			num *= IncidentWorker_QuestTradeRequest.ValueWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
			return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue)));
		}

		private static List<Thing> GenerateRewardsFor(ThingDef thingDef, int quantity, Faction faction, Map map)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = new FloatRange?(IncidentWorker_QuestTradeRequest.RewardValueFactorRange * IncidentWorker_QuestTradeRequest.RewardValueFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal) * thingDef.BaseMarketValue * (float)quantity);
			parms.validator = ((ThingDef td) => td != thingDef);
			List<Thing> list = null;
			for (int i = 0; i < 10; i++)
			{
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						list[j].Destroy(DestroyMode.Vanish);
					}
				}
				list = ThingSetMakerDefOf.Reward_TradeRequest.root.Generate(parms);
				float num = 0f;
				for (int k = 0; k < list.Count; k++)
				{
					num += list[k].MarketValue * (float)list[k].stackCount;
				}
				if (num > thingDef.BaseMarketValue * (float)quantity)
				{
					break;
				}
			}
			return list;
		}

		private int RandomOfferDurationTicks(int tileIdFrom, int tileIdTo)
		{
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			int num = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
			float num2 = (float)num / 60000f;
			int num3 = Mathf.CeilToInt(Mathf.Max(num2 + 6f, num2 * 1.35f));
			if (num3 > SiteTuning.QuestSiteTimeoutDaysRange.max)
			{
				return -1;
			}
			int num4 = Mathf.Max(randomInRange, num3);
			return 60000 * num4;
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
