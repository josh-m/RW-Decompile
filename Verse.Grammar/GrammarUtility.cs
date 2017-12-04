using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse.Grammar
{
	public static class GrammarUtility
	{
		public static IEnumerable<Rule> RulesForPawn(string prefix, Pawn pawn, Dictionary<string, string> constants = null)
		{
			if (pawn == null)
			{
				Log.ErrorOnce(string.Format("Tried to insert rule {0} for null pawn", prefix), 16015097);
				return Enumerable.Empty<Rule>();
			}
			if (pawn.RaceProps.Humanlike)
			{
				return GrammarUtility.RulesForPawn(prefix, pawn.Name, pawn.kindDef, pawn.gender, pawn.Faction, constants);
			}
			return GrammarUtility.RulesForPawn(prefix, null, pawn.kindDef, pawn.gender, pawn.Faction, constants);
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForPawn(string prefix, Name name, PawnKindDef kind, Gender gender, Faction faction, Dictionary<string, string> constants = null)
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
			if (constants != null && kind != null)
			{
				constants[prefix + "_flesh"] = kind.race.race.FleshType.defName;
			}
			if (faction != null)
			{
				yield return new Rule_String(prefix + "_factionName", faction.Name);
			}
			yield return new Rule_String(prefix + "_pronoun", gender.GetPronoun());
			yield return new Rule_String(prefix + "_possessive", gender.GetPossessive());
			yield return new Rule_String(prefix + "_objective", gender.GetObjective());
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForDef(string prefix, Def def)
		{
			if (def == null)
			{
				Log.ErrorOnce(string.Format("Tried to insert rule {0} for null def", prefix), 79641686);
			}
			else
			{
				yield return new Rule_String(prefix + "_label", def.label);
				yield return new Rule_String(prefix + "_labelDefinite", Find.ActiveLanguageWorker.WithDefiniteArticle(def.label));
				yield return new Rule_String(prefix + "_labelIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label));
				yield return new Rule_String(prefix + "_possessive", "Proits".Translate());
				HediffDef hediffDef = def as HediffDef;
				if (hediffDef != null)
				{
					string noun = hediffDef.labelNoun;
					if (noun.NullOrEmpty())
					{
						noun = hediffDef.label;
					}
					yield return new Rule_String(prefix + "_labelNoun", noun);
				}
			}
		}
	}
}
