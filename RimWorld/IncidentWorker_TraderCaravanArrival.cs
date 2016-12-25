using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_TraderCaravanArrival : IncidentWorker_NeutralGroup
	{
		protected override bool FactionCanBeGroupSource(Faction f, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, desperate) && f.def.caravanTraderKinds.Any<TraderKindDef>();
		}

		public override bool TryExecute(IncidentParms parms)
		{
			if (!this.TryResolveParms(parms))
			{
				return false;
			}
			List<Pawn> list = base.SpawnPawns(parms);
			if (list.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].needs != null && list[i].needs.food != null)
				{
					list[i].needs.food.CurLevel = list[i].needs.food.MaxLevel;
				}
			}
			TraderKindDef traderKindDef = null;
			foreach (Pawn current in list)
			{
				if (current.TraderKind != null)
				{
					traderKindDef = current.TraderKind;
				}
			}
			Find.LetterStack.ReceiveLetter("LetterLabelTraderCaravanArrival".Translate(new object[]
			{
				parms.faction.Name,
				traderKindDef.label
			}).CapitalizeFirst(), "LetterTraderCaravanArrival".Translate(new object[]
			{
				parms.faction.Name,
				traderKindDef.label
			}).CapitalizeFirst(), LetterType.Good, list[0], null);
			IntVec3 chillSpot;
			RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out chillSpot);
			LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(parms.faction, chillSpot);
			LordMaker.MakeNewLord(parms.faction, lordJob, list);
			return true;
		}

		protected override bool TryResolveParms(IncidentParms parms)
		{
			if (!base.TryResolveParms(parms))
			{
				return false;
			}
			parms.traderCaravan = true;
			return true;
		}
	}
}
