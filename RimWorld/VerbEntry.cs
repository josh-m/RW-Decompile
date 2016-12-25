using System;
using Verse;

namespace RimWorld
{
	public struct VerbEntry
	{
		public Verb verb;

		private int cachedDamage;

		public float SelectionWeight
		{
			get
			{
				return (float)this.cachedDamage;
			}
		}

		public VerbEntry(Verb verb, Pawn pawn, Thing equipment = null)
		{
			this.verb = verb;
			this.cachedDamage = verb.verbProps.AdjustedMeleeDamageAmount(verb, pawn, equipment);
		}

		public override string ToString()
		{
			return this.verb.ToString() + " - " + this.cachedDamage;
		}
	}
}
