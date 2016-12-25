using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.Grammar
{
	public static class GrammarUtility
	{
		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForPawn(string prefix, Name name, PawnKindDef kind, Gender gender, Faction faction = null)
		{
			string nameFull;
			if (name != null)
			{
				nameFull = name.ToStringFull;
			}
			else
			{
				nameFull = Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label);
			}
			yield return new Rule_String(prefix + "_nameFull", nameFull);
			string nameShort;
			if (name != null)
			{
				nameShort = name.ToStringShort;
			}
			else
			{
				nameShort = kind.label;
			}
			yield return new Rule_String(prefix + "_nameShort", nameShort);
			string nameShortIndef;
			if (name != null)
			{
				nameShortIndef = name.ToStringShort;
			}
			else
			{
				nameShortIndef = Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label);
			}
			yield return new Rule_String(prefix + "_nameShortIndef", nameShortIndef);
			yield return new Rule_String(prefix + "_nameShortIndefinite", nameShortIndef);
			string nameShortDef;
			if (name != null)
			{
				nameShortDef = name.ToStringShort;
			}
			else
			{
				nameShortDef = Find.ActiveLanguageWorker.WithDefiniteArticle(kind.label);
			}
			yield return new Rule_String(prefix + "_nameShortDef", nameShortDef);
			yield return new Rule_String(prefix + "_nameShortDefinite", nameShortDef);
			if (faction != null)
			{
				yield return new Rule_String(prefix + "_factionName", faction.Name);
			}
			yield return new Rule_String(prefix + "_pronoun", gender.GetPronoun());
			yield return new Rule_String(prefix + "_possessive", gender.GetPossessive());
			yield return new Rule_String(prefix + "_objective", gender.GetObjective());
		}
	}
}
