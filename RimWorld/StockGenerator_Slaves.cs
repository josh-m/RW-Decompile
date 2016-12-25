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
		public override IEnumerable<Thing> GenerateThings(Map forMap)
		{
			if (Rand.Value < Find.Storyteller.intenderPopulation.PopulationIntent)
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
					PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Slave, slaveFaction, PawnGenerationContext.NonPlayer, forMap, false, false, false, false, true, false, 1f, !this.trader.orbital, true, true, null, null, null, null, null, null);
					yield return PawnGenerator.GeneratePawn(request);
				}
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike;
		}
	}
}
