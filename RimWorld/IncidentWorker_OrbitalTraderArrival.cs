using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_OrbitalTraderArrival : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.passingShipManager.passingShips.Count >= 5)
			{
				return false;
			}
			TraderKindDef def;
			if ((from x in DefDatabase<TraderKindDef>.AllDefs
			where x.orbital
			select x).TryRandomElementByWeight((TraderKindDef traderDef) => traderDef.commonality, out def))
			{
				TradeShip tradeShip = new TradeShip(def);
				if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
				{
					Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(new object[]
					{
						tradeShip.name,
						tradeShip.def.label
					}), LetterDefOf.PositiveEvent, null);
				}
				map.passingShipManager.AddShip(tradeShip);
				tradeShip.GenerateThings();
				return true;
			}
			throw new InvalidOperationException();
		}
	}
}
