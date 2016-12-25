using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IBillGiver
	{
		Map Map
		{
			get;
		}

		BillStack BillStack
		{
			get;
		}

		IEnumerable<IntVec3> IngredientStackCells
		{
			get;
		}

		bool CurrentlyUsable();
	}
}
