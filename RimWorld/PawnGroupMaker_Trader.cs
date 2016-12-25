using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnGroupMaker_Trader : PawnGroupMaker
	{
		private const float GuardsPointsPerMarketValue = 0.015f;

		private const float MinGuardsPoints = 130f;

		private const float MaxGuardsPoints = 1700f;

		public List<PawnGenOption> traders = new List<PawnGenOption>();

		public List<PawnGenOption> carriers = new List<PawnGenOption>();

		public List<PawnGenOption> guards = new List<PawnGenOption>();

		public override float MinPointsToGenerateAnything
		{
			get
			{
				return 0f;
			}
		}

		public override bool CanGenerateFrom(IncidentParms parms)
		{
			bool arg_52_0;
			if (parms.traderCaravan && base.CanGenerateFrom(parms) && this.traders.Any<PawnGenOption>())
			{
				arg_52_0 = this.carriers.Any((PawnGenOption x) => Find.Map.Biome.IsPackAnimalAllowed(x.kind.race));
			}
			else
			{
				arg_52_0 = false;
			}
			return arg_52_0;
		}

		[DebuggerHidden]
		public override IEnumerable<Pawn> GenerateArrivingPawns(IncidentParms parms, bool errorOnZeroResults = true)
		{
			if (!this.CanGenerateFrom(parms))
			{
				if (errorOnZeroResults)
				{
					Log.Error("Cannot generate arriving trader caravan for " + parms.faction + ".");
				}
			}
			else if (!parms.faction.def.caravanTraderKinds.Any<TraderKindDef>())
			{
				Log.Error("Cannot generate arriving trader caravan for " + parms.faction + " because it has no trader kinds.");
			}
			else if (this.ValidateTradersAndCarriers(parms.faction))
			{
				TraderKindDef traderKind = (parms.traderKind == null) ? parms.faction.def.caravanTraderKinds.RandomElement<TraderKindDef>() : parms.traderKind;
				Pawn trader = this.GenerateTrader(parms, traderKind);
				yield return trader;
				List<Thing> wares = TraderStockGenerator.GenerateTraderThings(traderKind).InRandomOrder(null).ToList<Thing>();
				foreach (Pawn carrier in this.GenerateCarriers(parms, trader, wares))
				{
					yield return carrier;
				}
				foreach (Pawn slave in this.GetSlavesFromWares(parms, trader, wares))
				{
					yield return slave;
				}
				foreach (Pawn guard in this.GenerateGuards(parms, trader, wares))
				{
					yield return guard;
				}
				this.FinishedGeneratingPawns();
			}
		}

		private Pawn GenerateTrader(IncidentParms parms, TraderKindDef traderKind)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(this.traders.RandomElementByWeight((PawnGenOption x) => (float)x.selectionWeight).kind, parms.faction);
			pawn.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
			pawn.trader.traderKind = traderKind;
			this.PostGenerate(pawn);
			return pawn;
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> GenerateCarriers(IncidentParms parms, Pawn trader, List<Thing> wares)
		{
			List<Thing> nonPawnWares = (from x in wares
			where !(x is Pawn)
			select x).ToList<Thing>();
			int curItemIdx = 0;
			int carriersCount = Mathf.CeilToInt((float)nonPawnWares.Count / 8f);
			PawnKindDef carriersKind = (from x in this.carriers
			where Find.Map.Biome.IsPackAnimalAllowed(x.kind.race)
			select x).RandomElementByWeight((PawnGenOption x) => (float)x.selectionWeight).kind;
			List<Pawn> generatedCarriers = new List<Pawn>();
			for (int i = 0; i < carriersCount; i++)
			{
				Pawn carrier = PawnGenerator.GeneratePawn(carriersKind, parms.faction);
				this.PostGenerate(carrier);
				if (curItemIdx < nonPawnWares.Count)
				{
					carrier.inventory.container.TryAdd(nonPawnWares[curItemIdx]);
					curItemIdx++;
				}
				generatedCarriers.Add(carrier);
			}
			while (curItemIdx < nonPawnWares.Count)
			{
				generatedCarriers.RandomElement<Pawn>().inventory.container.TryAdd(nonPawnWares[curItemIdx]);
				curItemIdx++;
			}
			for (int j = 0; j < generatedCarriers.Count; j++)
			{
				yield return generatedCarriers[j];
			}
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> GetSlavesFromWares(IncidentParms parms, Pawn trader, List<Thing> wares)
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
		private IEnumerable<Pawn> GenerateGuards(IncidentParms parms, Pawn trader, List<Thing> wares)
		{
			if (this.guards.Any<PawnGenOption>())
			{
				float totalMarketValue = wares.Sum((Thing x) => x.MarketValue * (float)x.stackCount);
				float guardsPoints = totalMarketValue * 0.015f * Rand.Range(0.85f, 1.15f);
				guardsPoints = Mathf.Clamp(guardsPoints, 130f, 1700f);
				foreach (PawnGenOption g in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(guardsPoints, this.guards, parms))
				{
					PawnGenerationRequest req = new PawnGenerationRequest(g.kind, parms.faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, true, 1f, false, true, true, null, null, null, null, null, null);
					Pawn p = PawnGenerator.GeneratePawn(req);
					this.PostGenerate(p);
					yield return p;
				}
			}
		}

		private bool ValidateTradersAndCarriers(Faction faction)
		{
			PawnGenOption pawnGenOption = this.traders.FirstOrDefault((PawnGenOption x) => !x.kind.trader);
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
			PawnGenOption pawnGenOption2 = this.carriers.FirstOrDefault((PawnGenOption x) => !x.kind.carrier);
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
