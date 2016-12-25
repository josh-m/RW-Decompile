using System;
using System.Collections.Generic;

namespace Verse
{
	public abstract class HediffGiver
	{
		public HediffDef hediff;

		public List<BodyPartDef> partsToAffect;

		public bool canAffectAnyLivePart;

		public int countToAffect = 1;

		public virtual bool CheckGiveEverySecond(Pawn pawn)
		{
			return false;
		}

		public virtual bool CheckGiveHediffAdded(Pawn pawn, Hediff hediff)
		{
			return false;
		}

		public bool TryApply(Pawn pawn, List<Hediff> outAddedHediffs = null)
		{
			return HediffGiveUtility.TryApply(pawn, this.hediff, this.partsToAffect, this.canAffectAnyLivePart, this.countToAffect, outAddedHediffs);
		}
	}
}
