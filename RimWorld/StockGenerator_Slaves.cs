using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Slaves : StockGenerator
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			if (Rand.Value <= Find.Storyteller.intenderPopulation.PopulationIntent)
			{
				int count = this.countRange.RandomInRange;
				for (int i = 0; i < count; i++)
				{
					Faction slaveFaction;
					if (!(from fac in Find.FactionManager.AllFactionsVisible
					where fac != Faction.OfPlayer && fac.def.humanlikeFaction
					select fac).TryRandomElement(out slaveFaction))
					{
						break;
					}
					PawnKindDef slave = PawnKindDefOf.Slave;
					Faction faction = slaveFaction;
					PawnGenerationRequest request = new PawnGenerationRequest(slave, faction, PawnGenerationContext.NonPlayer, forTile, false, false, false, false, true, false, 1f, !this.trader.orbital, true, true, false, false, false, false, null, null, null, null, null, null, null);
					yield return PawnGenerator.GeneratePawn(request);
				}
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability != Tradeability.Never;
		}
	}
}
