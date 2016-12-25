using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IBillGiver
	{
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
