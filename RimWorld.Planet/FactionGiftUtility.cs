using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class FactionGiftUtility
	{
		private static readonly Texture2D OfferGiftsCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/OfferGifts", true);

		public static Command OfferGiftsCommand(Caravan caravan, SettlementBase settlement)
		{
			return new Command_Action
			{
				defaultLabel = "CommandOfferGifts".Translate(),
				defaultDesc = "CommandOfferGiftsDesc".Translate(),
				icon = FactionGiftUtility.OfferGiftsCommandTex,
				action = delegate
				{
					Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
					Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, settlement, true));
				}
			};
		}

		public static void GiveGift(List<Tradeable> tradeables, Faction giveTo, GlobalTargetInfo lookTarget)
		{
			int goodwillChange = FactionGiftUtility.GetGoodwillChange(tradeables, giveTo);
			for (int i = 0; i < tradeables.Count; i++)
			{
				if (tradeables[i].ActionToDo == TradeAction.PlayerSells)
				{
					tradeables[i].ResolveTrade();
				}
			}
			Faction ofPlayer = Faction.OfPlayer;
			int goodwillChange2 = goodwillChange;
			string reason = "GoodwillChangedReason_ReceivedGift".Translate();
			GlobalTargetInfo? lookTarget2 = new GlobalTargetInfo?(lookTarget);
			if (!giveTo.TryAffectGoodwillWith(ofPlayer, goodwillChange2, true, true, reason, lookTarget2))
			{
				FactionGiftUtility.SendGiftNotAppreciatedMessage(giveTo, lookTarget);
			}
		}

		public static void GiveGift(List<ActiveDropPodInfo> pods, SettlementBase giveTo)
		{
			int goodwillChange = FactionGiftUtility.GetGoodwillChange(pods.Cast<IThingHolder>(), giveTo);
			for (int i = 0; i < pods.Count; i++)
			{
				ThingOwner innerContainer = pods[i].innerContainer;
				for (int j = innerContainer.Count - 1; j >= 0; j--)
				{
					FactionGiftUtility.GiveGiftInternal(innerContainer[j], innerContainer[j].stackCount, giveTo.Faction);
					if (j < innerContainer.Count)
					{
						innerContainer.RemoveAt(j);
					}
				}
			}
			Faction arg_AE_0 = giveTo.Faction;
			Faction ofPlayer = Faction.OfPlayer;
			int goodwillChange2 = goodwillChange;
			string reason = "GoodwillChangedReason_ReceivedGift".Translate();
			GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(giveTo);
			if (!arg_AE_0.TryAffectGoodwillWith(ofPlayer, goodwillChange2, true, true, reason, lookTarget))
			{
				FactionGiftUtility.SendGiftNotAppreciatedMessage(giveTo.Faction, giveTo);
			}
		}

		private static void GiveGiftInternal(Thing thing, int count, Faction giveTo)
		{
			Thing thing2 = thing.SplitOff(count);
			Pawn pawn = thing2 as Pawn;
			if (pawn != null)
			{
				pawn.SetFaction(giveTo, null);
			}
			thing2.DestroyOrPassToWorld(DestroyMode.Vanish);
		}

		public static bool CheckCanCarryGift(List<Tradeable> tradeables, ITrader trader)
		{
			Pawn pawn = trader as Pawn;
			if (pawn == null)
			{
				return true;
			}
			float num = 0f;
			float num2 = 0f;
			Lord lord = pawn.GetLord();
			if (lord != null)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn2 = lord.ownedPawns[i];
					TraderCaravanRole traderCaravanRole = pawn2.GetTraderCaravanRole();
					if ((pawn2.RaceProps.Humanlike && traderCaravanRole != TraderCaravanRole.Guard) || traderCaravanRole == TraderCaravanRole.Carrier)
					{
						num += MassUtility.Capacity(pawn2, null);
						num2 += MassUtility.GearAndInventoryMass(pawn2);
					}
				}
			}
			else
			{
				num = MassUtility.Capacity(pawn, null);
				num2 = MassUtility.GearAndInventoryMass(pawn);
			}
			float num3 = 0f;
			for (int j = 0; j < tradeables.Count; j++)
			{
				if (tradeables[j].ActionToDo == TradeAction.PlayerSells)
				{
					int num4 = Mathf.Min(tradeables[j].CountToTransferToDestination, tradeables[j].CountHeldBy(Transactor.Colony));
					if (num4 > 0)
					{
						num3 += tradeables[j].AnyThing.GetStatValue(StatDefOf.Mass, true) * (float)num4;
					}
				}
			}
			if (num2 + num3 <= num)
			{
				return true;
			}
			float num5 = num - num2;
			if (num5 <= 0f)
			{
				Messages.Message("MessageCantGiveGiftBecauseCantCarryEncumbered".Translate(), MessageTypeDefOf.RejectInput, false);
			}
			else
			{
				Messages.Message("MessageCantGiveGiftBecauseCantCarry".Translate(num3.ToStringMass(), num5.ToStringMass()), MessageTypeDefOf.RejectInput, false);
			}
			return false;
		}

		public static int GetGoodwillChange(IEnumerable<IThingHolder> pods, SettlementBase giveTo)
		{
			float num = 0f;
			foreach (IThingHolder current in pods)
			{
				ThingOwner directlyHeldThings = current.GetDirectlyHeldThings();
				for (int i = 0; i < directlyHeldThings.Count; i++)
				{
					float singlePrice;
					if (directlyHeldThings[i].def == ThingDefOf.Silver)
					{
						singlePrice = directlyHeldThings[i].MarketValue;
					}
					else
					{
						float priceFactorSell_TraderPriceType = (giveTo.TraderKind == null) ? 1f : giveTo.TraderKind.PriceTypeFor(directlyHeldThings[i].def, TradeAction.PlayerSells).PriceMultiplier();
						float tradePriceImprovementOffsetForPlayer = giveTo.TradePriceImprovementOffsetForPlayer;
						singlePrice = TradeUtility.GetPricePlayerSell(directlyHeldThings[i], priceFactorSell_TraderPriceType, 1f, tradePriceImprovementOffsetForPlayer);
					}
					num += FactionGiftUtility.GetBaseGoodwillChange(directlyHeldThings[i], directlyHeldThings[i].stackCount, singlePrice, giveTo.Faction);
				}
			}
			return FactionGiftUtility.PostProcessedGoodwillChange(num, giveTo.Faction);
		}

		public static int GetGoodwillChange(List<Tradeable> tradeables, Faction theirFaction)
		{
			float num = 0f;
			for (int i = 0; i < tradeables.Count; i++)
			{
				if (tradeables[i].ActionToDo == TradeAction.PlayerSells)
				{
					int count = Mathf.Min(tradeables[i].CountToTransferToDestination, tradeables[i].CountHeldBy(Transactor.Colony));
					num += FactionGiftUtility.GetBaseGoodwillChange(tradeables[i].AnyThing, count, tradeables[i].GetPriceFor(TradeAction.PlayerSells), theirFaction);
				}
			}
			return FactionGiftUtility.PostProcessedGoodwillChange(num, theirFaction);
		}

		private static float GetBaseGoodwillChange(Thing anyThing, int count, float singlePrice, Faction theirFaction)
		{
			if (count <= 0)
			{
				return 0f;
			}
			float num = singlePrice * (float)count;
			Pawn pawn = anyThing as Pawn;
			if (pawn != null && pawn.IsPrisoner && pawn.Faction == theirFaction)
			{
				num *= 2f;
			}
			return num / 40f;
		}

		private static int PostProcessedGoodwillChange(float goodwillChange, Faction theirFaction)
		{
			float num = (float)theirFaction.PlayerGoodwill;
			float num2 = 0f;
			SimpleCurve giftGoodwillFactorRelationsCurve = DiplomacyTuning.GiftGoodwillFactorRelationsCurve;
			while (goodwillChange >= 0.25f)
			{
				num2 += 0.25f * giftGoodwillFactorRelationsCurve.Evaluate(Mathf.Min(num + num2, 100f));
				goodwillChange -= 0.25f;
				if (num2 >= 200f)
				{
					break;
				}
			}
			if (num2 < 200f)
			{
				num2 += goodwillChange * giftGoodwillFactorRelationsCurve.Evaluate(Mathf.Min(num + num2, 100f));
			}
			return (int)Mathf.Min(num2, 200f);
		}

		private static void SendGiftNotAppreciatedMessage(Faction giveTo, GlobalTargetInfo lookTarget)
		{
			Messages.Message("MessageGiftGivenButNotAppreciated".Translate(giveTo.Name).CapitalizeFirst(), lookTarget, MessageTypeDefOf.NegativeEvent, true);
		}
	}
}
