using System;
using Verse;

namespace RimWorld
{
	public struct VerbEntry
	{
		public Verb verb;

		private float cachedSelectionWeight;

		public float SelectionWeight
		{
			get
			{
				return this.cachedSelectionWeight;
			}
		}

		public bool IsMeleeAttack
		{
			get
			{
				return this.verb.IsMeleeAttack;
			}
		}

		public VerbEntry(Verb verb, Pawn pawn, Thing equipment = null)
		{
			this.verb = verb;
			this.cachedSelectionWeight = verb.verbProps.AdjustedMeleeSelectionWeight(verb, pawn, equipment);
		}

		public override string ToString()
		{
			return this.verb.ToString() + " - " + this.cachedSelectionWeight;
		}
	}
}
