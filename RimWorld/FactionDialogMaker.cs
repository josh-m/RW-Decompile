using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FactionDialogMaker
	{
		private const float MinRelationsToCommunicate = -70f;

		private const float MinRelationsFriendly = 40f;

		private const int GiftSilverAmount = 300;

		private const float GiftSilverGoodwillChange = 12f;

		private const float MilitaryAidRelsChange = -25f;

		private const int TradeRequestCost_Wary = 1100;

		private const int TradeRequestCost_Warm = 700;

		private static DiaNode root;

		private static Pawn negotiator;

		private static Faction faction;

		public static DiaNode FactionDialogFor(Pawn negotiator, Faction faction)
		{
			Map map = negotiator.Map;
			FactionDialogMaker.negotiator = negotiator;
			FactionDialogMaker.faction = faction;
			string text = (faction.leader != null) ? faction.leader.Name.ToStringFull : faction.Name;
			if (faction.PlayerGoodwill < -70f)
			{
				FactionDialogMaker.root = new DiaNode("FactionGreetingHostile".Translate(new object[]
				{
					text
				}));
			}
			else if (faction.PlayerGoodwill < 40f)
			{
				string text2 = "FactionGreetingWary".Translate(new object[]
				{
					text,
					negotiator.LabelShort
				});
				text2 = text2.AdjustedFor(negotiator);
				FactionDialogMaker.root = new DiaNode(text2);
				if (!SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
				{
					FactionDialogMaker.root.options.Add(FactionDialogMaker.OfferGiftOption(negotiator.Map));
				}
				if (!faction.HostileTo(Faction.OfPlayer) && negotiator.Spawned && negotiator.Map.IsPlayerHome)
				{
					FactionDialogMaker.root.options.Add(FactionDialogMaker.RequestTraderOption(map, 1100));
				}
			}
			else
			{
				FactionDialogMaker.root = new DiaNode("FactionGreetingWarm".Translate(new object[]
				{
					text,
					negotiator.LabelShort
				}));
				if (!SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
				{
					FactionDialogMaker.root.options.Add(FactionDialogMaker.OfferGiftOption(negotiator.Map));
				}
				if (!faction.HostileTo(Faction.OfPlayer) && negotiator.Spawned && negotiator.Map.IsPlayerHome)
				{
					FactionDialogMaker.root.options.Add(FactionDialogMaker.RequestTraderOption(map, 700));
					FactionDialogMaker.root.options.Add(FactionDialogMaker.RequestMilitaryAidOption(map));
				}
			}
			if (Prefs.DevMode)
			{
				foreach (DiaOption current in FactionDialogMaker.DebugOptions())
				{
					FactionDialogMaker.root.options.Add(current);
				}
			}
			DiaOption diaOption = new DiaOption("(" + "Disconnect".Translate() + ")");
			diaOption.resolveTree = true;
			FactionDialogMaker.root.options.Add(diaOption);
			return FactionDialogMaker.root;
		}

		[DebuggerHidden]
		private static IEnumerable<DiaOption> DebugOptions()
		{
			DiaOption opt = new DiaOption("(Debug) Goodwill +10");
			opt.action = delegate
			{
				FactionDialogMaker.faction.AffectGoodwillWith(Faction.OfPlayer, 10f);
			};
			opt.linkLateBind = (() => FactionDialogMaker.FactionDialogFor(FactionDialogMaker.negotiator, FactionDialogMaker.faction));
			yield return opt;
			DiaOption opt2 = new DiaOption("(Debug) Goodwill -10");
			opt2.action = delegate
			{
				FactionDialogMaker.faction.AffectGoodwillWith(Faction.OfPlayer, -10f);
			};
			opt2.linkLateBind = (() => FactionDialogMaker.FactionDialogFor(FactionDialogMaker.negotiator, FactionDialogMaker.faction));
			yield return opt2;
		}

		private static int AmountSendableSilver(Map map)
		{
			return (from t in TradeUtility.AllLaunchableThings(map)
			where t.def == ThingDefOf.Silver
			select t).Sum((Thing t) => t.stackCount);
		}

		private static DiaOption OfferGiftOption(Map map)
		{
			if (FactionDialogMaker.AmountSendableSilver(map) < 300)
			{
				DiaOption diaOption = new DiaOption("OfferGift".Translate());
				diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
				{
					300
				}));
				return diaOption;
			}
			float goodwillDelta = 12f * FactionDialogMaker.negotiator.GetStatValue(StatDefOf.GiftImpact, true);
			DiaOption diaOption2 = new DiaOption("OfferGift".Translate() + " (" + "SilverForGoodwill".Translate(new object[]
			{
				300,
				goodwillDelta.ToString("#####0")
			}) + ")");
			diaOption2.action = delegate
			{
				TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 300, map, null);
				FactionDialogMaker.faction.AffectGoodwillWith(Faction.OfPlayer, goodwillDelta);
			};
			string text = "SilverGiftSent".Translate(new object[]
			{
				FactionDialogMaker.faction.leader.LabelIndefinite(),
				Mathf.RoundToInt(goodwillDelta)
			}).CapitalizeFirst();
			diaOption2.link = new DiaNode(text)
			{
				options = 
				{
					FactionDialogMaker.OKToRoot()
				}
			};
			return diaOption2;
		}

		private static DiaOption RequestTraderOption(Map map, int silverCost)
		{
			string text = "RequestTrader".Translate(new object[]
			{
				silverCost.ToString()
			});
			if (FactionDialogMaker.AmountSendableSilver(map) < silverCost)
			{
				DiaOption diaOption = new DiaOption(text);
				diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
				{
					silverCost
				}));
				return diaOption;
			}
			if (!FactionDialogMaker.faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
			{
				DiaOption diaOption2 = new DiaOption(text);
				diaOption2.Disable("BadTemperature".Translate());
				return diaOption2;
			}
			int num = FactionDialogMaker.faction.lastTraderRequestTick + 240000 - Find.TickManager.TicksGame;
			if (num > 0)
			{
				DiaOption diaOption3 = new DiaOption(text);
				diaOption3.Disable("WaitTime".Translate(new object[]
				{
					num.ToStringTicksToPeriod(true, false, true)
				}));
				return diaOption3;
			}
			DiaOption diaOption4 = new DiaOption(text);
			DiaNode diaNode = new DiaNode("TraderSent".Translate(new object[]
			{
				FactionDialogMaker.faction.leader.LabelIndefinite()
			}).CapitalizeFirst());
			diaNode.options.Add(FactionDialogMaker.OKToRoot());
			DiaNode diaNode2 = new DiaNode("ChooseTraderKind".Translate(new object[]
			{
				FactionDialogMaker.faction.leader.LabelIndefinite()
			}));
			foreach (TraderKindDef current in FactionDialogMaker.faction.def.caravanTraderKinds)
			{
				TraderKindDef localTk = current;
				DiaOption diaOption5 = new DiaOption(localTk.LabelCap);
				diaOption5.action = delegate
				{
					IncidentParms incidentParms = new IncidentParms();
					incidentParms.target = map;
					incidentParms.faction = FactionDialogMaker.faction;
					incidentParms.traderKind = localTk;
					incidentParms.forced = true;
					Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival, Find.TickManager.TicksGame + 120000, incidentParms);
					FactionDialogMaker.faction.lastTraderRequestTick = Find.TickManager.TicksGame;
					TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverCost, map, null);
				};
				diaOption5.link = diaNode;
				diaNode2.options.Add(diaOption5);
			}
			DiaOption diaOption6 = new DiaOption("GoBack".Translate());
			diaOption6.linkLateBind = FactionDialogMaker.ResetToRoot();
			diaNode2.options.Add(diaOption6);
			diaOption4.link = diaNode2;
			return diaOption4;
		}

		private static DiaOption RequestMilitaryAidOption(Map map)
		{
			string text = "RequestMilitaryAid".Translate(new object[]
			{
				-25f
			});
			if (!FactionDialogMaker.faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
			{
				DiaOption diaOption = new DiaOption(text);
				diaOption.Disable("BadTemperature".Translate());
				return diaOption;
			}
			DiaOption diaOption2 = new DiaOption(text);
			if (map.attackTargetsCache.TargetsHostileToColony.Any((IAttackTarget x) => !x.ThreatDisabled()))
			{
				if (!map.attackTargetsCache.TargetsHostileToColony.Any((IAttackTarget p) => ((Thing)p).Faction != null && ((Thing)p).Faction.HostileTo(FactionDialogMaker.faction)))
				{
					IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
					where !x.ThreatDisabled()
					select x into pa
					select ((Thing)pa).Faction into fa
					where fa != null && !fa.HostileTo(FactionDialogMaker.faction)
					select fa).Distinct<Faction>();
					string arg_1B6_0 = "MilitaryAidConfirmMutualEnemy";
					object[] expr_17D = new object[2];
					expr_17D[0] = FactionDialogMaker.faction.Name;
					expr_17D[1] = GenText.ToCommaList(from fa in source
					select fa.Name, true);
					DiaNode diaNode = new DiaNode(arg_1B6_0.Translate(expr_17D));
					DiaOption diaOption3 = new DiaOption("CallConfirm".Translate());
					diaOption3.action = delegate
					{
						FactionDialogMaker.CallForAid(map);
					};
					diaOption3.link = FactionDialogMaker.FightersSent();
					DiaOption diaOption4 = new DiaOption("CallCancel".Translate());
					diaOption4.linkLateBind = FactionDialogMaker.ResetToRoot();
					diaNode.options.Add(diaOption3);
					diaNode.options.Add(diaOption4);
					diaOption2.link = diaNode;
					return diaOption2;
				}
			}
			diaOption2.action = delegate
			{
				FactionDialogMaker.CallForAid(map);
			};
			diaOption2.link = FactionDialogMaker.FightersSent();
			return diaOption2;
		}

		private static DiaNode FightersSent()
		{
			return new DiaNode("MilitaryAidSent".Translate(new object[]
			{
				FactionDialogMaker.faction.leader.LabelIndefinite()
			}).CapitalizeFirst())
			{
				options = 
				{
					FactionDialogMaker.OKToRoot()
				}
			};
		}

		private static void CallForAid(Map map)
		{
			FactionDialogMaker.faction.AffectGoodwillWith(Faction.OfPlayer, -25f);
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = map;
			incidentParms.faction = FactionDialogMaker.faction;
			incidentParms.points = (float)Rand.Range(150, 400);
			IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
		}

		private static DiaOption OKToRoot()
		{
			return new DiaOption("OK".Translate())
			{
				linkLateBind = FactionDialogMaker.ResetToRoot()
			};
		}

		private static Func<DiaNode> ResetToRoot()
		{
			return () => FactionDialogMaker.FactionDialogFor(FactionDialogMaker.negotiator, FactionDialogMaker.faction);
		}
	}
}
