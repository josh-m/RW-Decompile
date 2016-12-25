using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_OrbitalTraderArrival : IncidentWorker
	{
		public override bool TryExecute(IncidentParms parms)
		{
			if (Find.PassingShipManager.passingShips.Count >= 5)
			{
				return false;
			}
			TraderKindDef def;
			if ((from x in DefDatabase<TraderKindDef>.AllDefs
			where x.orbital
			select x).TryRandomElement(out def))
			{
				TradeShip tradeShip = new TradeShip(def);
				if (Find.ListerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
				{
					Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(new object[]
					{
						tradeShip.name,
						tradeShip.def.label
					}), LetterType.Good, null);
				}
				Find.PassingShipManager.AddShip(tradeShip);
				tradeShip.GenerateThings();
				return true;
			}
			throw new InvalidOperationException();
		}
	}
}
