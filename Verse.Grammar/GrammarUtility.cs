using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse.Grammar
{
	public static class GrammarUtility
	{
		public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Pawn pawn, Dictionary<string, string> constants = null)
		{
			if (pawn == null)
			{
				Log.ErrorOnce(string.Format("Tried to insert rule {0} for null pawn", pawnSymbol), 16015097, false);
				return Enumerable.Empty<Rule>();
			}
			return GrammarUtility.RulesForPawn(pawnSymbol, pawn.Name, (pawn.story == null) ? null : pawn.story.Title, pawn.kindDef, pawn.gender, pawn.Faction, constants);
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Name name, string title, PawnKindDef kind, Gender gender, Faction faction, Dictionary<string, string> constants = null)
		{
			string nameFull;
			if (name != null)
			{
				nameFull = Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringFull, gender, false, true);
			}
			else
			{
				nameFull = Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label, gender, false, false);
			}
			yield return new Rule_String(pawnSymbol + "_nameFull", nameFull);
			string nameShort;
			if (name != null)
			{
				nameShort = name.ToStringShort;
			}
			else
			{
				nameShort = kind.label;
			}
			yield return new Rule_String(pawnSymbol + "_label", nameShort);
			string nameShortDef;
			if (name != null)
			{
				nameShortDef = Find.ActiveLanguageWorker.WithDefiniteArticle(name.ToStringShort, gender, false, true);
			}
			else
			{
				nameShortDef = Find.ActiveLanguageWorker.WithDefiniteArticle(kind.label, gender, false, false);
			}
			yield return new Rule_String(pawnSymbol + "_definite", nameShortDef);
			yield return new Rule_String(pawnSymbol + "_nameDef", nameShortDef);
			string nameShortIndef;
			if (name != null)
			{
				nameShortIndef = Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringShort, gender, false, true);
			}
			else
			{
				nameShortIndef = Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label, gender, false, false);
			}
			yield return new Rule_String(pawnSymbol + "_indefinite", nameShortIndef);
			yield return new Rule_String(pawnSymbol + "_nameIndef", nameShortIndef);
			yield return new Rule_String(pawnSymbol + "_pronoun", gender.GetPronoun());
			yield return new Rule_String(pawnSymbol + "_possessive", gender.GetPossessive());
			yield return new Rule_String(pawnSymbol + "_objective", gender.GetObjective());
			if (faction != null)
			{
				yield return new Rule_String(pawnSymbol + "_factionName", faction.Name);
			}
			if (kind != null)
			{
				yield return new Rule_String(pawnSymbol + "_kind", GenLabel.BestKindLabel(kind, gender, false, -1));
			}
			if (title != null)
			{
				yield return new Rule_String(pawnSymbol + "_title", title);
			}
			if (constants != null && kind != null)
			{
				constants[pawnSymbol + "_flesh"] = kind.race.race.FleshType.defName;
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForDef(string prefix, Def def)
		{
			if (def == null)
			{
				Log.ErrorOnce(string.Format("Tried to insert rule {0} for null def", prefix), 79641686, false);
			}
			else
			{
				yield return new Rule_String(prefix + "_label", def.label);
				yield return new Rule_String(prefix + "_definite", Find.ActiveLanguageWorker.WithDefiniteArticle(def.label, false, false));
				yield return new Rule_String(prefix + "_indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label, false, false));
				yield return new Rule_String(prefix + "_possessive", "Proits".Translate());
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForBodyPartRecord(string prefix, BodyPartRecord part)
		{
			if (part == null)
			{
				Log.ErrorOnce(string.Format("Tried to insert rule {0} for null body part", prefix), 394876778, false);
			}
			else
			{
				yield return new Rule_String(prefix + "_label", part.Label);
				yield return new Rule_String(prefix + "_definite", Find.ActiveLanguageWorker.WithDefiniteArticle(part.Label, false, false));
				yield return new Rule_String(prefix + "_indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(part.Label, false, false));
				yield return new Rule_String(prefix + "_possessive", "Proits".Translate());
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForHediffDef(string prefix, HediffDef def, BodyPartRecord part)
		{
			foreach (Rule rule in GrammarUtility.RulesForDef(prefix, def))
			{
				yield return rule;
			}
			string noun = def.labelNoun;
			if (noun.NullOrEmpty())
			{
				noun = def.label;
			}
			yield return new Rule_String(prefix + "_labelNoun", noun);
			string pretty = def.PrettyTextForPart(part);
			if (!pretty.NullOrEmpty())
			{
				yield return new Rule_String(prefix + "_labelNounPretty", pretty);
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForFaction(string prefix, Faction faction)
		{
			if (faction == null)
			{
				yield return new Rule_String(prefix + "_name", "FactionUnaffiliated".Translate());
			}
			else
			{
				yield return new Rule_String(prefix + "_name", faction.Name);
			}
		}
	}
}
