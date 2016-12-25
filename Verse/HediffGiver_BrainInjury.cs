using RimWorld;
using System;

namespace Verse
{
	public class HediffGiver_BrainInjury : HediffGiver
	{
		public float chancePerDamagePct;

		public string letterLabel;

		public string letter;

		public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
		{
			if (!(hediff is Hediff_Injury))
			{
				return false;
			}
			if (hediff.Part != pawn.health.hediffSet.GetBrain())
			{
				return false;
			}
			float num = hediff.Severity / hediff.Part.def.GetMaxHealth(pawn);
			if (Rand.Value < num * this.chancePerDamagePct)
			{
				bool flag = base.TryApply(pawn, null);
				if (flag)
				{
					if ((pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony) && !this.letter.NullOrEmpty())
					{
						Find.LetterStack.ReceiveLetter(this.letterLabel, this.letter.AdjustedFor(pawn), LetterType.BadNonUrgent, pawn, null);
					}
					return true;
				}
			}
			return false;
		}
	}
}
