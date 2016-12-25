using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnGroupKindWorker_Trader : PawnGroupKindWorker
	{
		private const float GuardsPointsPerMarketValue = 0.015f;

		private const float MinGuardsPoints = 130f;

		private const float MaxGuardsPoints = 1700f;

		public override float MinPointsToGenerateAnything(PawnGroupMaker groupMaker)
		{
			return 0f;
		}

		public override bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			return base.CanGenerateFrom(parms, groupMaker) && groupMaker.traders.Any<PawnGenOption>() && (parms.map == null || groupMaker.carriers.Any((PawnGenOption x) => parms.map.Biome.IsPackAnimalAllowed(x.kind.race)));
		}

		[DebuggerHidden]
		public override IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults = true)
		{
			if (!this.CanGenerateFrom(parms, groupMaker))
			{
				if (errorOnZeroResults)
				{
					Log.Error("Cannot generate trader caravan for " + parms.faction + ".");
				}
			}
			else if (!parms.faction.def.caravanTraderKinds.Any<TraderKindDef>())
			{
				Log.Error("Cannot generate trader caravan for " + parms.faction + " because it has no trader kinds.");
			}
			else if (this.ValidateTradersAndCarriers(parms.faction, groupMaker))
			{
				TraderKindDef traderKind = (parms.traderKind == null) ? parms.faction.def.caravanTraderKinds.RandomElement<TraderKindDef>() : parms.traderKind;
				Pawn trader = this.GenerateTrader(parms, groupMaker, traderKind);
				yield return trader;
				List<Thing> wares = TraderStockGenerator.GenerateTraderThings(traderKind, parms.map).InRandomOrder(null).ToList<Thing>();
				foreach (Pawn carrier in this.GenerateCarriers(parms, groupMaker, trader, wares))
				{
					yield return carrier;
				}
				foreach (Pawn slave in this.GetSlavesAndAnimalsFromWares(parms, trader, wares))
				{
					yield return slave;
				}
				foreach (Pawn guard in this.GenerateGuards(parms, groupMaker, trader, wares))
				{
					yield return guard;
				}
				this.FinishedGeneratingPawns();
			}
		}

		private Pawn GenerateTrader(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, TraderKindDef traderKind)
		{
			Map map = parms.map;
			PawnGenerationRequest request = new PawnGenerationRequest(groupMaker.traders.RandomElementByWeight((PawnGenOption x) => (float)x.selectionWeight).kind, parms.faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			pawn.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
			pawn.trader.traderKind = traderKind;
			this.PostGenerate(pawn);
			return pawn;
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> GenerateCarriers(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares)
		{
			List<Thing> nonPawnWares = (from x in wares
			where !(x is Pawn)
			select x).ToList<Thing>();
			int curItemIdx = 0;
			int carriersCount = Mathf.CeilToInt((float)nonPawnWares.Count / 8f);
			PawnKindDef carriersKind = (from x in groupMaker.carriers
			where this.parms.map == null || this.parms.map.Biome.IsPackAnimalAllowed(x.kind.race)
			select x).RandomElementByWeight((PawnGenOption x) => (float)x.selectionWeight).kind;
			List<Pawn> generatedCarriers = new List<Pawn>();
			for (int i = 0; i < carriersCount; i++)
			{
				Map map = parms.map;
				PawnGenerationRequest request = new PawnGenerationRequest(carriersKind, parms.faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
				Pawn carrier = PawnGenerator.GeneratePawn(request);
				this.PostGenerate(carrier);
				if (curItemIdx < nonPawnWares.Count)
				{
					carrier.inventory.innerContainer.TryAdd(nonPawnWares[curItemIdx], true);
					curItemIdx++;
				}
				generatedCarriers.Add(carrier);
			}
			while (curItemIdx < nonPawnWares.Count)
			{
				generatedCarriers.RandomElement<Pawn>().inventory.innerContainer.TryAdd(nonPawnWares[curItemIdx], true);
				curItemIdx++;
			}
			for (int j = 0; j < generatedCarriers.Count; j++)
			{
				yield return generatedCarriers[j];
			}
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> GetSlavesAndAnimalsFromWares(PawnGroupMakerParms parms, Pawn trader, List<Thing> wares)
		{
			for (int i = 0; i < wares.Count; i++)
			{
				Pawn p = wares[i] as Pawn;
				if (p != null)
				{
					if (p.Faction != parms.faction)
					{
						p.SetFaction(parms.faction, null);
					}
					yield return p;
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> GenerateGuards(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares)
		{
			if (groupMaker.guards.Any<PawnGenOption>())
			{
				float totalMarketValue = wares.Sum((Thing x) => x.MarketValue * (float)x.stackCount);
				float guardsPoints = totalMarketValue * 0.015f * Rand.Range(0.85f, 1.15f);
				guardsPoints = Mathf.Clamp(guardsPoints, 130f, 1700f);
				foreach (PawnGenOption g in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(guardsPoints, groupMaker.guards, parms))
				{
					Map map = parms.map;
					PawnGenerationRequest request = new PawnGenerationRequest(g.kind, parms.faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, true, 1f, false, true, true, null, null, null, null, null, null);
					Pawn p = PawnGenerator.GeneratePawn(request);
					this.PostGenerate(p);
					yield return p;
				}
			}
		}

		private bool ValidateTradersAndCarriers(Faction faction, PawnGroupMaker groupMaker)
		{
			PawnGenOption pawnGenOption = groupMaker.traders.FirstOrDefault((PawnGenOption x) => !x.kind.trader);
			if (pawnGenOption != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Cannot generate arriving trader caravan for ",
					faction,
					" because there is a pawn kind (",
					pawnGenOption.kind.LabelCap,
					") who is not a trader but is in a traders list."
				}));
				return false;
			}
			PawnGenOption pawnGenOption2 = groupMaker.carriers.FirstOrDefault((PawnGenOption x) => !x.kind.RaceProps.packAnimal);
			if (pawnGenOption2 != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Cannot generate arriving trader caravan for ",
					faction,
					" because there is a pawn kind (",
					pawnGenOption2.kind.LabelCap,
					") who is not a carrier but is in a carriers list."
				}));
				return false;
			}
			return true;
		}
	}
}
