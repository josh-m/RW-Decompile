using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanVisitUtility
	{
		private static readonly Texture2D TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Trade", true);

		public static Settlement SettlementVisitedNow(Caravan caravan)
		{
			if (!caravan.Spawned || caravan.pather.Moving)
			{
				return null;
			}
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			for (int i = 0; i < settlements.Count; i++)
			{
				Settlement settlement = settlements[i];
				if (settlement.Tile == caravan.Tile && settlement.Faction != caravan.Faction && settlement.Visitable)
				{
					return settlement;
				}
			}
			return null;
		}

		public static Command TradeCommand(Caravan caravan)
		{
			Pawn bestNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandTrade".Translate();
			command_Action.defaultDesc = "CommandTradeDesc".Translate();
			command_Action.icon = CaravanVisitUtility.TradeCommandTex;
			command_Action.action = delegate
			{
				Settlement settlement = CaravanVisitUtility.SettlementVisitedNow(caravan);
				if (settlement != null && settlement.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, settlement));
					string empty = string.Empty;
					string empty2 = string.Empty;
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(settlement.Goods.OfType<Pawn>(), ref empty, ref empty2, "LetterRelatedPawnsTradingWithSettlement".Translate(), false, true);
					if (!empty2.NullOrEmpty())
					{
						Find.LetterStack.ReceiveLetter(empty, empty2, LetterDefOf.NeutralEvent, settlement, null);
					}
				}
			};
			if (bestNegotiator == null)
			{
				command_Action.Disable("CommandTradeFailNoNegotiator".Translate());
			}
			if (bestNegotiator != null && bestNegotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
			{
				command_Action.Disable("CommandTradeFailSocialDisabled".Translate());
			}
			return command_Action;
		}

		public static Command FulfillRequestCommand(Caravan caravan)
		{
			Func<Thing, bool> validator = (Thing thing) => thing.GetRotStage() == RotStage.Fresh;
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandFulfillTradeOffer".Translate();
			command_Action.defaultDesc = "CommandFulfillTradeOfferDesc".Translate();
			command_Action.icon = CaravanVisitUtility.TradeCommandTex;
			command_Action.action = delegate
			{
				Settlement settlement2 = CaravanVisitUtility.SettlementVisitedNow(caravan);
				CaravanRequestComp caravanRequest = (settlement2 == null) ? null : settlement2.GetComponent<CaravanRequestComp>();
				if (caravanRequest != null)
				{
					if (!caravanRequest.ActiveRequest)
					{
						Log.Error("Attempted to fulfill an unavailable request");
						return;
					}
					if (!CaravanInventoryUtility.HasThings(caravan, caravanRequest.requestThingDef, caravanRequest.requestCount, validator))
					{
						Messages.Message("CommandFulfillTradeOfferFailInsufficient".Translate(new object[]
						{
							GenLabel.ThingLabel(caravanRequest.requestThingDef, null, caravanRequest.requestCount)
						}), MessageTypeDefOf.RejectInput);
						return;
					}
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandFulfillTradeOfferConfirm".Translate(new object[]
					{
						GenLabel.ThingLabel(caravanRequest.requestThingDef, null, caravanRequest.requestCount),
						caravanRequest.rewards[0].Label
					}), delegate
					{
						int remaining = caravanRequest.requestCount;
						List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate(Thing thing)
						{
							if (caravanRequest.requestThingDef != thing.def)
							{
								return 0;
							}
							int num = Mathf.Min(remaining, thing.stackCount);
							remaining -= num;
							return num;
						});
						for (int i = 0; i < list.Count; i++)
						{
							list[i].Destroy(DestroyMode.Vanish);
						}
						while (caravanRequest.rewards.Count > 0)
						{
							Thing thing2 = caravanRequest.rewards[caravanRequest.rewards.Count - 1];
							caravanRequest.rewards.Remove(thing2);
							CaravanInventoryUtility.GiveThing(caravan, thing2);
						}
						caravanRequest.Disable();
					}, false, null));
				}
			};
			Settlement settlement = CaravanVisitUtility.SettlementVisitedNow(caravan);
			CaravanRequestComp caravanRequestComp = (settlement == null) ? null : settlement.GetComponent<CaravanRequestComp>();
			if (!CaravanInventoryUtility.HasThings(caravan, caravanRequestComp.requestThingDef, caravanRequestComp.requestCount, validator))
			{
				command_Action.Disable("CommandFulfillTradeOfferFailInsufficient".Translate(new object[]
				{
					GenLabel.ThingLabel(caravanRequestComp.requestThingDef, null, caravanRequestComp.requestCount)
				}));
			}
			return command_Action;
		}
	}
}
