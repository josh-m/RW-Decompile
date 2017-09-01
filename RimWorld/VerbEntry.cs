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

		public VerbEntry(Verb verb, Pawn pawn, Thing equipment = null)
		{
			this.verb = verb;
			this.cachedSelectionWeight = verb.verbProps.AdjustedSelectionWeight(verb, pawn, equipment);
		}

		public override string ToString()
		{
			return this.verb.ToString() + " - " + this.cachedSelectionWeight;
		}
	}
}
