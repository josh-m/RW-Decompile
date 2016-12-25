using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class MentalStateWorker_BingingDrug : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			if (!pawn.Spawned)
			{
				return false;
			}
			List<ChemicalDef> allDefsListForReading = DefDatabase<ChemicalDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (AddictionUtility.CanBingeOnNow(pawn, allDefsListForReading[i], this.def.drugCategory))
				{
					return true;
				}
				if (this.def.drugCategory == DrugCategory.Hard && AddictionUtility.CanBingeOnNow(pawn, allDefsListForReading[i], DrugCategory.Social))
				{
					return true;
				}
			}
			return false;
		}
	}
}
