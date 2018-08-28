using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ExpectationsUtility
	{
		private static List<ExpectationDef> expectationsInOrder;

		public static void Reset()
		{
			ExpectationsUtility.expectationsInOrder = (from ed in DefDatabase<ExpectationDef>.AllDefs
			orderby ed.maxMapWealth
			select ed).ToList<ExpectationDef>();
		}

		public static ExpectationDef CurrentExpectationFor(Pawn p)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return null;
			}
			if (p.Faction != Faction.OfPlayer && !p.IsPrisonerOfColony)
			{
				return ExpectationDefOf.ExtremelyLow;
			}
			if (p.MapHeld != null)
			{
				return ExpectationsUtility.CurrentExpectationFor(p.MapHeld);
			}
			return ExpectationDefOf.VeryLow;
		}

		public static ExpectationDef CurrentExpectationFor(Map m)
		{
			float wealthTotal = m.wealthWatcher.WealthTotal;
			for (int i = 0; i < ExpectationsUtility.expectationsInOrder.Count; i++)
			{
				ExpectationDef expectationDef = ExpectationsUtility.expectationsInOrder[i];
				if (wealthTotal < expectationDef.maxMapWealth)
				{
					return expectationDef;
				}
			}
			return ExpectationsUtility.expectationsInOrder[ExpectationsUtility.expectationsInOrder.Count - 1];
		}
	}
}
